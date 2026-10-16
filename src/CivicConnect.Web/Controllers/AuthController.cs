using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CivicConnect.Application.Services;
using CivicConnect.Domain.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace CivicConnect.Web.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<TokenResult>> Register([FromBody] RegisterDto dto)
    {
        try
        {
            var user = await _authService.RegisterAsync(dto);
            var result = BuildTokenResult(user);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "MANAGER")]
    [HttpPost("users")]
    public async Task<ActionResult<ProvisionedUserDto>> ProvisionUser([FromBody] ProvisionDto dto)
    {
        // Login tokens carry a name; development demo tokens deliberately do not.
        var actor = User.FindFirstValue(ClaimTypes.Name);
        if (string.IsNullOrWhiteSpace(actor))
        {
            return Forbid();
        }

        try
        {
            var user = await _authService.ProvisionUserAsync(dto, actor);
            return Created($"/api/auth/users/{user.Id}", new ProvisionedUserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role
            });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPost("login")]
    public async Task<ActionResult<TokenResult>> Login([FromBody] LoginDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Username) || string.IsNullOrWhiteSpace(dto.Password))
        {
            return BadRequest(new { message = "Username and password are required." });
        }

        try
        {
            var user = await _authService.LoginAsync(dto);
            var result = BuildTokenResult(user);
            return Ok(result);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized(new { message = "Invalid username or password." });
        }
    }

    [Authorize]
    [HttpGet("session")]
    public IActionResult GetSession()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub");
        var role = User.FindFirstValue(ClaimTypes.Role);

        return string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(role)
            ? Unauthorized()
            : Ok(new { userId, role });
    }

    private static TokenResult BuildTokenResult(User user)
    {
        var issuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? "CivicConnect";
        var audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? "CivicConnectUsers";
        var secret = Environment.GetEnvironmentVariable("JWT_SECRET")
                     ?? throw new InvalidOperationException("JWT_SECRET environment variable is not set.");

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Username),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.NameIdentifier, user.Username),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role.ToUpperInvariant())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: creds
        );

        return new TokenResult
        {
            UserId = user.Username,
            Role = user.Role,
            Token = new JwtSecurityTokenHandler().WriteToken(token)
        };
    }
}

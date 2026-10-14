#if DEBUG
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CivicConnect.Web.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TestAuthController : ControllerBase
{
    private readonly IConfiguration _config;
    public TestAuthController(IConfiguration config) => _config = config;

    [HttpGet("token/{userId}")]
    public IActionResult GenerateToken(string userId, string? role = "Requester")
    {
        var claims = new[]
        {
              new Claim(JwtRegisteredClaimNames.Sub, userId),
              new Claim(ClaimTypes.Role, role ?? "Requester")
          };

        var secret = Environment.GetEnvironmentVariable("JWT_SECRET");
        if (string.IsNullOrWhiteSpace(secret))
        {
            return Problem("JWT_SECRET is not configured.", statusCode: 500);
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: Environment.GetEnvironmentVariable("JWT_ISSUER"),
            audience: Environment.GetEnvironmentVariable("JWT_AUDIENCE"),
            claims: claims,
            expires: DateTime.Now.AddDays(1),
            signingCredentials: creds
        );

        return Ok(new { token = new JwtSecurityTokenHandler().WriteToken(token) });
    }
}
#endif
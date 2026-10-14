using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CivicConnect.Tests;

/// <summary>
/// Test-only authentication handler that translates <c>X-Test-Role</c> / <c>X-Test-User</c>
/// headers into an authenticated principal.
/// </summary>
public class TestAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string HeaderRole = "X-Test-Role";
    public const string HeaderUser = "X-Test-User";

    public TestAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(HeaderRole, out var roles))
            return Task.FromResult(AuthenticateResult.NoResult());

        var role = roles.ToString().ToUpperInvariant() switch
        {
            "REQUESTER" => "Requester",
            "STAFF" => "Staff",
            "MANAGER" => "Manager",
            "ANONYMOUS" => "Anonymous",
            _ => roles.ToString()
        };
        if (role == "Anonymous")
            return Task.FromResult(AuthenticateResult.NoResult());

        var userId = Request.Headers.TryGetValue(HeaderUser, out var users)
            ? users.ToString()
            : string.Empty;

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId ?? string.Empty),
            new(ClaimTypes.Name, userId ?? string.Empty),
            new(ClaimTypes.Role, role)
        };

        var identity = new ClaimsIdentity(claims, "Test");
        var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), Scheme.Name);
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}

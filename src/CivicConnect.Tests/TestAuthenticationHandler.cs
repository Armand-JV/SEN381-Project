using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CivicConnect.Tests;

/// <summary>
/// Replaces JWT bearer authentication in integration tests. Reads test-only role and user
/// headers and maps them to an authenticated principal.
/// </summary>
public sealed class TestAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string SchemeName = "TestScheme";

    public TestAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(TestAuthenticationHelper.RoleHeaderName, out var role) ||
            string.IsNullOrWhiteSpace(role))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var normalizedRole = role.ToString().ToUpperInvariant();
        if (normalizedRole == "ANONYMOUS")
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var userId = Request.Headers[TestAuthenticationHelper.UserHeaderName].ToString();
        var claims = new List<Claim>
        {
            new(ClaimTypes.Role, normalizedRole)
        };
        if (!string.IsNullOrWhiteSpace(userId))
        {
            claims.Add(new Claim(ClaimTypes.NameIdentifier, userId));
            claims.Add(new Claim(ClaimTypes.Name, userId));
        }

        var identity = new ClaimsIdentity(claims, SchemeName);
        return Task.FromResult(AuthenticateResult.Success(
            new AuthenticationTicket(new ClaimsPrincipal(identity), SchemeName)));
    }
}

using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace CivicConnect.Web.Authentication;

public sealed class TestHeaderAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public TestHeaderAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var environment = Context.RequestServices.GetRequiredService<IHostEnvironment>();
        if (!environment.IsDevelopment() ||
            !Context.RequestServices.GetRequiredService<IConfiguration>().GetValue<bool>("DemoAuth:Enabled"))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var role = Request.Headers["X-Test-Role"].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(role))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        role = role.Trim().ToUpperInvariant();
        if (role is not ("REQUESTER" or "STAFF" or "MANAGER"))
        {
            return Task.FromResult(AuthenticateResult.Fail("The supplied test role is not supported."));
        }

        var userId = Request.Headers["X-Test-User"].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Task.FromResult(AuthenticateResult.Fail("A test user ID is required."));
        }

        userId = userId.Trim();

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Name, userId),
            new Claim(ClaimTypes.Role, role)
        };
        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}

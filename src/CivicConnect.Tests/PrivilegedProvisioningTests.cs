using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using CivicConnect.Application.Services;
using CivicConnect.Domain.Entities;
using CivicConnect.Web.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CivicConnect.Tests;

public sealed class PrivilegedProvisioningTests : IClassFixture<PrivilegedProvisioningTests.Factory>
{
    private readonly Factory _factory;

    public PrivilegedProvisioningTests(Factory factory)
    {
        _factory = factory;
        _factory.ResetDatabase();
    }

    [Theory]
    [InlineData("Staff")]
    [InlineData("Manager")]
    public async Task ManagerCanProvisionPrivilegedUser_WithAuditAndSafeResponse(string role)
    {
        using var client = _factory.CreateClient();
        await BootstrapManagerAsync();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer", await LoginAsync(client, "manager-one", "Password123!"));

        var response = await client.PostAsJsonAsync("/api/auth/users", new ProvisionDto
        {
            Username = $"provisioned-{role.ToLowerInvariant()}",
            Email = $"provisioned-{role.ToLowerInvariant()}@example.com",
            Password = "Password123!",
            Role = role
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("Password123!", body);
        Assert.DoesNotContain("passwordHash", body, StringComparison.OrdinalIgnoreCase);

        await using var db = _factory.CreateDbContext();
        var user = await db.Users.SingleAsync(candidate => candidate.Username == $"provisioned-{role.ToLowerInvariant()}");
        Assert.Equal(role, user.Role);
        Assert.Equal($"provisioned-{role.ToLowerInvariant()}@example.com", user.Email);
        Assert.True(BCrypt.Net.BCrypt.Verify("Password123!", user.PasswordHash));
        var audit = await db.AuditLogs.SingleAsync(entry =>
            entry.Action == "UserProvisioned" && entry.EntityId == user.Id);
        Assert.Equal("manager-one", audit.Actor);
        Assert.Equal($"Role:{role}", audit.AfterSummary);
    }

    [Fact]
    public async Task AnonymousAndRequesterCannotProvisionUsers()
    {
        using var anonymous = _factory.CreateClient();
        var dto = new ProvisionDto
        {
            Username = "staff-one",
            Email = "staff@example.com",
            Password = "Password123!",
            Role = "Staff"
        };

        Assert.Equal(HttpStatusCode.Unauthorized,
            (await anonymous.PostAsJsonAsync("/api/auth/users", dto)).StatusCode);

        var requester = await RegisterAsync(anonymous, "requester-one");
        anonymous.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", requester.Token);
        Assert.Equal(HttpStatusCode.Forbidden,
            (await anonymous.PostAsJsonAsync("/api/auth/users", dto)).StatusCode);
    }

    private async Task BootstrapManagerAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<AuthService>();
        await service.BootstrapManagerAsync(new ProvisionDto
        {
            Username = "manager-one",
            Email = "manager@example.com",
            Password = "Password123!",
            Role = "Manager"
        });
    }

    private static async Task<string> LoginAsync(HttpClient client, string username, string password)
    {
        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginDto
        {
            Username = username,
            Password = password
        });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<TokenResult>())!.Token;
    }

    private static async Task<TokenResult> RegisterAsync(HttpClient client, string username)
    {
        var response = await client.PostAsJsonAsync("/api/auth/register", new RegisterDto
        {
            Username = username,
            Email = $"{username}@example.com",
            Password = "Password123!",
            Role = "Requester"
        });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<TokenResult>())!;
    }

    public sealed class Factory : TestWebApplicationFactory
    {
        private readonly Dictionary<string, string?> _originalEnv = new();

        public Factory()
        {
            SetEnv("JWT_SECRET", "privileged-provisioning-test-secret-with-sufficient-length-123456");
            SetEnv("JWT_ISSUER", "CivicConnect");
            SetEnv("JWT_AUDIENCE", "CivicConnectUsers");
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            base.ConfigureWebHost(builder);
            builder.ConfigureServices(services => services.PostConfigure<AuthenticationOptions>(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultForbidScheme = JwtBearerDefaults.AuthenticationScheme;
            }));
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            foreach (var (name, value) in _originalEnv)
            {
                Environment.SetEnvironmentVariable(name, value);
            }
        }

        private void SetEnv(string name, string value)
        {
            _originalEnv[name] = Environment.GetEnvironmentVariable(name);
            Environment.SetEnvironmentVariable(name, value);
        }
    }
}

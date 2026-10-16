using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using CivicConnect.Application.Services;
using CivicConnect.Web.Controllers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace CivicConnect.Tests;

public sealed class JwtAuthFlowTests : IClassFixture<JwtAuthFlowTests.Factory>
{
    private readonly Factory _factory;

    public JwtAuthFlowTests(Factory factory)
    {
        _factory = factory;
        _factory.ResetDatabase();
    }

    [Fact]
    public async Task RegisterCreateListRead_WithRealJwt_PreservesOwnershipAndRoleRestrictions()
    {
        using var client = _factory.CreateClient();
        var username = $"jwt-{Guid.NewGuid():N}";
        var auth = await RegisterAsync(client, username);
        Assert.Equal(username, auth.UserId);
        Assert.Equal("Requester", auth.Role);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.Token);

        var created = await client.PostAsJsonAsync("/api/requests", new CreateRequestDto
        {
            Title = "JWT flow request",
            Description = "Created through the real JWT pipeline.",
            CategoryId = 1
        });
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var request = await created.Content.ReadFromJsonAsync<CreatedRequestDto>();
        Assert.NotNull(request);

        // A real JWT must pass the case-sensitive REQUESTER gate, unlike test headers.
        var mine = await client.GetFromJsonAsync<PagedResultDto<RequesterRequestListItemDto>>("/api/requests/mine");
        Assert.NotNull(mine);
        Assert.Contains(mine.Items, item => item.Id == request.Id);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync($"/api/requests/{request.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync($"/api/requests/{request.Id}/history")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await client.GetAsync("/api/staff/requests")).StatusCode);

        var login = await client.PostAsJsonAsync("/api/auth/login", new LoginDto
        {
            Username = username,
            Password = "Password123!"
        });
        login.EnsureSuccessStatusCode();
        var relogin = await login.Content.ReadFromJsonAsync<TokenResult>();
        Assert.NotNull(relogin);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", relogin.Token);
        var relisted = await client.GetFromJsonAsync<PagedResultDto<RequesterRequestListItemDto>>("/api/requests/mine");
        Assert.NotNull(relisted);
        Assert.Contains(relisted.Items, item => item.Id == request.Id);

        using var otherClient = _factory.CreateClient();
        var otherAuth = await RegisterAsync(otherClient, $"jwt-{Guid.NewGuid():N}");
        otherClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", otherAuth.Token);
        var otherMine = await otherClient.GetFromJsonAsync<PagedResultDto<RequesterRequestListItemDto>>("/api/requests/mine");
        Assert.NotNull(otherMine);
        Assert.DoesNotContain(otherMine.Items, item => item.Id == request.Id);
        Assert.Equal(HttpStatusCode.Forbidden, (await otherClient.GetAsync($"/api/requests/{request.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await otherClient.GetAsync($"/api/requests/{request.Id}/history")).StatusCode);
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
        var auth = await response.Content.ReadFromJsonAsync<TokenResult>();
        Assert.NotNull(auth);
        return auth;
    }

    public sealed class Factory : TestWebApplicationFactory
    {
        private readonly Dictionary<string, string?> _originalEnv = new();

        public Factory()
        {
            SetEnv("JWT_SECRET", "jwt-auth-flow-test-secret-with-sufficient-length-1234567890");
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

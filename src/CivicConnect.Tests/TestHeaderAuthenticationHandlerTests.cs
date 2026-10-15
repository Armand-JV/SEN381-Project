using System.Security.Claims;
using System.Text.Encodings.Web;
using CivicConnect.Web.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace CivicConnect.Tests;

public sealed class TestHeaderAuthenticationHandlerTests
{
    [Fact]
    public async Task DevelopmentWithDemoAuthAndHeaders_AuthenticatesExplicitUser()
    {
        var result = await Authenticate("Development", true, "STAFF", "staff-1");

        Assert.True(result.Succeeded);
        Assert.Equal("staff-1", result.Principal!.FindFirstValue(ClaimTypes.NameIdentifier));
        Assert.Equal("STAFF", result.Principal.FindFirstValue(ClaimTypes.Role));
    }

    [Fact]
    public async Task DevelopmentWithoutDemoAuth_ReturnsNoResult()
    {
        var result = await Authenticate("Development", false, "STAFF", "staff-1");

        Assert.False(result.Succeeded);
        Assert.Null(result.Failure);
    }

    [Fact]
    public async Task ProductionWithDemoAuth_ReturnsNoResult()
    {
        var result = await Authenticate("Production", true, "STAFF", "staff-1");

        Assert.False(result.Succeeded);
        Assert.Null(result.Failure);
    }

    [Fact]
    public async Task MissingUserHeader_FailsAuthentication()
    {
        var result = await Authenticate("Development", true, "STAFF", null);

        Assert.False(result.Succeeded);
        Assert.Equal("A test user ID is required.", result.Failure?.Message);
    }

    private static async Task<AuthenticateResult> Authenticate(
        string environmentName,
        bool demoAuthEnabled,
        string? role,
        string? userId)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IHostEnvironment>(new TestHostEnvironment(environmentName));
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["DemoAuth:Enabled"] = demoAuthEnabled.ToString()
            })
            .Build());
        using var provider = services.BuildServiceProvider();
        var context = new DefaultHttpContext { RequestServices = provider };
        if (role is not null) context.Request.Headers["X-Test-Role"] = role;
        if (userId is not null) context.Request.Headers["X-Test-User"] = userId;

        var options = new AuthenticationSchemeOptions();
        var monitor = new StaticOptionsMonitor<AuthenticationSchemeOptions>(options);
        var handler = new TestHeaderAuthenticationHandler(
            monitor,
            NullLoggerFactory.Instance,
            UrlEncoder.Default);
        await handler.InitializeAsync(new AuthenticationScheme("TestHeader", null, typeof(TestHeaderAuthenticationHandler)), context);
        return await handler.AuthenticateAsync();
    }

    private sealed class StaticOptionsMonitor<T>(T value) : IOptionsMonitor<T>
        where T : class
    {
        public T CurrentValue => value;
        public T Get(string? name) => value;
        public IDisposable? OnChange(Action<T, string?> listener) => null;
    }

    private sealed class TestHostEnvironment(string environmentName) : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = environmentName;
        public string ApplicationName { get; set; } = "CivicConnect.Tests";
        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;
        public Microsoft.Extensions.FileProviders.IFileProvider ContentRootFileProvider { get; set; } = new Microsoft.Extensions.FileProviders.NullFileProvider();
    }
}

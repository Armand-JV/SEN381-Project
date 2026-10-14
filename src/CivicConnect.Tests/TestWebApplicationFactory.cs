using CivicConnect.Web;
using CivicConnect.Web.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CivicConnect.Tests;

/// <summary>
/// Uses PostgreSQL when CI supplies a connection string; otherwise uses an in-memory
/// database so local controller tests stay hermetic.
/// </summary>
public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    private const string CiDatabaseName = "civicconnect_ci";
    private const string InMemoryDatabaseName = "CivicConnectTest";
    private readonly bool _usePostgres = !string.IsNullOrWhiteSpace(
        Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection"));

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        if (!_usePostgres && Environment.GetEnvironmentVariable("CI") == "true")
        {
            throw new InvalidOperationException("CI requires ConnectionStrings__DefaultConnection.");
        }

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<CivicConnectDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<CivicConnectDbContext>>();
            services.AddDbContext<CivicConnectDbContext>(options =>
            {
                if (_usePostgres)
                {
                    options.UseNpgsql(Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection"));
                }
                else
                {
                    options.UseInMemoryDatabase(InMemoryDatabaseName);
                }
            });

            services.AddAuthentication(TestAuthenticationHandler.SchemeName)
                .AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>(
                    TestAuthenticationHandler.SchemeName, null);
        });

        builder.UseEnvironment("Testing");
    }

    public void ResetDatabase()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CivicConnectDbContext>();

        if (!_usePostgres)
        {
            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();
            return;
        }

        if (!string.Equals(db.Database.GetDbConnection().Database, CiDatabaseName, StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                $"Refusing to reset non-CI database '{db.Database.GetDbConnection().Database}'.");
        }

        db.Requests.ExecuteDelete();
    }
}

using CivicConnect.Web;
using CivicConnect.Web.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace CivicConnect.Tests;

/// <summary>
/// Uses PostgreSQL when CI supplies a connection string; otherwise uses an in-memory
/// database so local controller tests stay hermetic.
/// </summary>
public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    private const string CiDatabaseName = "civicconnect_ci";
    private readonly string _inMemoryDatabaseName = Guid.NewGuid().ToString();
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
                    options.UseInMemoryDatabase(_inMemoryDatabaseName);
                }
            });

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = TestAuthenticationHandler.SchemeName;
                options.DefaultChallengeScheme = TestAuthenticationHandler.SchemeName;
                options.DefaultForbidScheme = TestAuthenticationHandler.SchemeName;
            })
                .AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>(
                    TestAuthenticationHandler.SchemeName, null);
        });

        builder.UseEnvironment("Testing");
    }

    public CivicConnectDbContext CreateDbContext()
    {
        var scope = Services.CreateScope();
        return scope.ServiceProvider.GetRequiredService<CivicConnectDbContext>();
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

        db.AuditLogs.ExecuteDelete();
        db.Notifications.ExecuteDelete();
        db.Users.ExecuteDelete();
        db.StatusHistories.ExecuteDelete();
        db.Feedback.ExecuteDelete();
        db.AuditEntries.ExecuteDelete();
        db.Comments.Where(comment => comment.Id != 1).ExecuteDelete();
        db.Requests.Where(request => request.Id != 2).ExecuteDelete();
    }
}

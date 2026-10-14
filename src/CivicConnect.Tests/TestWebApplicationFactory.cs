using CivicConnect.Web;
using CivicConnect.Web.Data;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

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
    private readonly string _inMemoryDatabaseName = Guid.NewGuid().ToString();
    internal const string JwtIssuer = "civicconnect-tests";
    internal const string JwtSecret = "test-only-signing-key-not-for-production-123456789";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        if (!_usePostgres && Environment.GetEnvironmentVariable("CI") == "true")
        {
            // Remove the production DbContext registrations (Npgsql).
            services.RemoveAll<DbContextOptions<CivicConnectDbContext>>();
            services.RemoveAll<DbContextOptions>();
            services.RemoveAll<IDbContextOptionsConfiguration<CivicConnectDbContext>>();
            services.RemoveAll<IConfigureOptions<DbContextOptions<CivicConnectDbContext>>>();

            // Add isolated in-memory DB and test signing key for JWT auth.
            services.AddDbContext<CivicConnectDbContext>(options =>
                options.UseInMemoryDatabase(_inMemoryDatabaseName));

            services.PostConfigure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtSecret)),
                    ValidateIssuer = true,
                    ValidIssuer = JwtIssuer,
                    ValidateAudience = true,
                    ValidAudience = JwtIssuer,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
            });
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

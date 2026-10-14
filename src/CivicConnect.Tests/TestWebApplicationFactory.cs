using CivicConnect.Web;
using CivicConnect.Web.Configuration;
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
/// Custom <see cref="WebApplicationFactory{TProgram}"/> used by integration tests.
///
/// Swaps the production Npgsql registration for an EF Core in-memory database so the
/// suite is hermetic and runs without a network dependency while still exercising the
/// real MVC pipeline, routing, controller construction and middleware.
/// </summary>
public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _inMemoryDatabaseName = Guid.NewGuid().ToString();
    internal const string JwtIssuer = "civicconnect-tests";
    internal const string JwtSecret = "test-only-signing-key-not-for-production-123456789";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
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

    /// <summary>
    /// Returns a scoped <see cref="CivicConnectDbContext"/> backed by the test in-memory
    /// database, useful for seeding data or asserting persisted state.
    /// </summary>
    public CivicConnectDbContext CreateDbContext()
    {
        var scope = Services.CreateScope();
        return scope.ServiceProvider.GetRequiredService<CivicConnectDbContext>();
    }

    /// <summary>
    /// Deletes and re-creates the in-memory database so each test starts from a known state.
    /// </summary>
    public void ResetDatabase()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CivicConnectDbContext>();
        db.Database.EnsureDeleted();
        db.Database.EnsureCreated();
    }
}

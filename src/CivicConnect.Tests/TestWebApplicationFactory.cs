using System.Linq;
using CivicConnect.Web;
using CivicConnect.Web.Configuration;
using CivicConnect.Web.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

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
    private const string InMemoryDatabaseName = "CivicConnectTest";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove the production DbContext registrations (Npgsql).
            var descriptor = services.FirstOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<CivicConnectDbContext>));
            if (descriptor != null)
                services.Remove(descriptor);

            // Add the in-memory replacement.
            services.AddDbContext<CivicConnectDbContext>(options =>
                options.UseInMemoryDatabase(InMemoryDatabaseName));
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

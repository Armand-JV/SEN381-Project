using System;
using System.Threading.Tasks;
using CivicConnect.Web.Configuration;
using CivicConnect.Web.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace CivicConnect.Tests;

public class DatabaseConnectionTests
{
    [Fact]
    public async Task CanConnectToDatabase_WhenConfiguredWithEnv()
    {
        var isCi = Environment.GetEnvironmentVariable("CI") == "true";
        if (!isCi)
        {
            EnvLoader.Load();
        }

        var configuration = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .Build();
        var configuredConnectionString = configuration.GetConnectionString("DefaultConnection");
        var hasLegacyDatabaseConfiguration = !string.IsNullOrWhiteSpace(
            Environment.GetEnvironmentVariable("DB_IP")) &&
            !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("DB_NAME"));

        if (string.IsNullOrWhiteSpace(configuredConnectionString) && (isCi || !hasLegacyDatabaseConfiguration))
        {
            throw new InvalidOperationException(
                "Database configuration is missing. Set ConnectionStrings__DefaultConnection or DB_IP and DB_NAME.");
        }

        var connectionString = !string.IsNullOrWhiteSpace(configuredConnectionString)
            ? configuredConnectionString
            : EnvLoader.GetConnectionString(configuration);
        var optionsBuilder = new DbContextOptionsBuilder<CivicConnectDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        await using var context = new CivicConnectDbContext(optionsBuilder.Options);
        var canConnect = await context.Database.CanConnectAsync();

        Assert.True(canConnect, "Failed to connect to the configured PostgreSQL database.");
    }
}

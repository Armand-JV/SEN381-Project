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
        // Arrange
        EnvLoader.Load();

        var configuration = new ConfigurationBuilder().Build();
        var connectionString = EnvLoader.GetConnectionString(configuration);

        // Fail loudly if DB_IP/DB_NAME were not provided (would fall back to localhost).
        var dbIp = Environment.GetEnvironmentVariable("DB_IP");
        var dbName = Environment.GetEnvironmentVariable("DB_NAME");
        if (string.IsNullOrEmpty(dbIp) || string.IsNullOrEmpty(dbName))
        {
            throw new InvalidOperationException(
                "Database configuration is missing. Set DB_IP and DB_NAME environment variables " +
                "(or repository secrets) so the test connects to the remote PostgreSQL instance, not localhost.");
        }

        var optionsBuilder = new DbContextOptionsBuilder<CivicConnectDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        await using var context = new CivicConnectDbContext(optionsBuilder.Options);

        // Act
        var canConnect = await context.Database.CanConnectAsync();

        // Assert
        var displayConnectionString = connectionString;
        var dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD");
        if (!string.IsNullOrEmpty(dbPassword))
        {
            displayConnectionString = connectionString.Replace(dbPassword, "******");
        }

        Assert.True(canConnect, $"Failed to connect to database using connection string: {displayConnectionString}");
    }
}

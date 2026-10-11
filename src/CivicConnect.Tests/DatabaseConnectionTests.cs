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

        var optionsBuilder = new DbContextOptionsBuilder<CivicConnectDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        await using var context = new CivicConnectDbContext(optionsBuilder.Options);

        // Act
        var canConnect = await context.Database.CanConnectAsync();

        // Assert
        Assert.True(canConnect, $"Failed to connect to database using connection string: {connectionString.Replace(Environment.GetEnvironmentVariable("DB_PASSWORD") ?? "", "******")}");
    }
}

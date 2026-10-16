using CivicConnect.Domain.Entities;
using CivicConnect.Web.Configuration;
using CivicConnect.Web.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace CivicConnect.Tests;

public class UserStoreTests
{
    [Fact]
    public async Task FindByEmailAsync_NormalizesInput_AndExecutesOnPostgres()
    {
        if (Environment.GetEnvironmentVariable("CI") != "true")
        {
            EnvLoader.Load();
        }

        var configuration = new ConfigurationBuilder().AddEnvironmentVariables().Build();
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? EnvLoader.GetConnectionString(configuration);
        var options = new DbContextOptionsBuilder<CivicConnectDbContext>()
            .UseNpgsql(connectionString).Options;
        await using var db = new CivicConnectDbContext(options);
        await using var transaction = await db.Database.BeginTransactionAsync();
        var user = new User
        {
            Username = $"email-test-{Guid.NewGuid():N}",
            Email = $"{Guid.NewGuid():N}@example.com",
            Role = "Requester",
            CreatedAt = DateTime.UtcNow
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var found = await new UserStore(db).FindByEmailAsync($"  {user.Email.ToUpperInvariant()}  ");

        Assert.NotNull(found);
        Assert.Equal(user.Id, found.Id);
        Assert.Equal(user.Email, found.Email);
        await transaction.RollbackAsync();
    }
}

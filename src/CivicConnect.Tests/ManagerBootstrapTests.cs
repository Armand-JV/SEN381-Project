using CivicConnect.Application.Services;
using CivicConnect.Web.Configuration;
using CivicConnect.Web.Data;
using Microsoft.EntityFrameworkCore;

namespace CivicConnect.Tests;

public sealed class ManagerBootstrapTests : IDisposable
{
    private readonly CivicConnectDbContext _db = new(new DbContextOptionsBuilder<CivicConnectDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    [Fact]
    public async Task RunAsync_SkipsWhenNoSecretsConfigured()
    {
        await ManagerBootstrap.RunAsync(new AuthService(new UserStore(_db)), null, null, null);
        Assert.Empty(_db.Users);
    }

    [Fact]
    public async Task RunAsync_RejectsPartialConfiguration()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            ManagerBootstrap.RunAsync(new AuthService(new UserStore(_db)), "manager", null, null));
        Assert.Empty(_db.Users);
    }

    [Fact]
    public async Task RunAsync_CreatesOnlyFirstManagerWithoutResettingCredentials()
    {
        var service = new AuthService(new UserStore(_db));
        await ManagerBootstrap.RunAsync(service, "manager-one", "manager@example.com", "Password123!");
        var user = Assert.Single(_db.Users);
        var originalHash = user.PasswordHash;
        Assert.True(BCrypt.Net.BCrypt.Verify("Password123!", user.PasswordHash));

        await ManagerBootstrap.RunAsync(service, "manager-two", "other@example.com", "AnotherPassword123!");
        Assert.Single(_db.Users);
        Assert.Equal(originalHash, user.PasswordHash);
        var audit = Assert.Single(_db.AuditLogs);
        Assert.Equal("system:bootstrap", audit.Actor);
        Assert.Equal(user.Id, audit.EntityId);
    }

    public void Dispose() => _db.Dispose();
}

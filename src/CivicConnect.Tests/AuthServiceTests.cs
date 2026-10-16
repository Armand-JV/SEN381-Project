using CivicConnect.Application.Interfaces;
using CivicConnect.Application.Services;
using CivicConnect.Domain.Entities;

namespace CivicConnect.Tests;

public class AuthServiceTests
{
    [Fact]
    public async Task RegisterAsync_StoresRequesterWithHashedPassword()
    {
        var store = new InMemoryUserStore();
        var service = new AuthService(store);

        var user = await service.RegisterAsync(new RegisterDto
        {
            Username = "test-user",
            Email = "TEST@example.com",
            Password = "Password123!",
            Role = "requester"
        });

        Assert.Equal("test-user", user.Username);
        Assert.Equal("test@example.com", user.Email);
        Assert.Equal("Requester", user.Role);
        Assert.NotEqual("Password123!", user.PasswordHash);
        Assert.True(BCrypt.Net.BCrypt.Verify("Password123!", user.PasswordHash));
    }

    [Fact]
    public async Task RegisterAsync_RejectsStaffRole()
    {
        var service = new AuthService(new InMemoryUserStore());

        var error = await Assert.ThrowsAsync<ArgumentException>(() => service.RegisterAsync(new RegisterDto
        {
            Username = "staff-user",
            Email = "staff@example.com",
            Password = "Password123!",
            Role = "Staff"
        }));

        Assert.Contains("Requester", error.Message);
    }

    [Theory]
    [InlineData("Staff")]
    [InlineData("Manager")]
    public async Task ProvisionUserAsync_ManagerCreatesPrivilegedAccountAndAudit(string role)
    {
        var store = new InMemoryUserStore();
        var service = new AuthService(store);
        await service.BootstrapManagerAsync(new ProvisionDto
        {
            Username = "manager-one",
            Email = "manager@example.com",
            Password = "Password123!",
            Role = "Manager"
        });

        var user = await service.ProvisionUserAsync(new ProvisionDto
        {
            Username = $"new-{role.ToLowerInvariant()}",
            Email = $"new-{role.ToLowerInvariant()}@example.com",
            Password = "Password123!",
            Role = role
        }, "manager-one");

        Assert.Equal(role, user.Role);
        Assert.NotEqual("Password123!", user.PasswordHash);
        var audit = Assert.Single(store.Audits, entry => entry.Actor == "manager-one");
        Assert.Equal(user.Id, audit.EntityId);
        Assert.Equal("UserProvisioned", audit.Action);
        Assert.DoesNotContain("Password123!", audit.AfterSummary ?? string.Empty);
    }

    [Fact]
    public async Task ProvisionUserAsync_NonManagerActorIsRejected()
    {
        var store = new InMemoryUserStore();
        var service = new AuthService(store);
        await service.RegisterAsync(new RegisterDto
        {
            Username = "requester-one",
            Email = "requester@example.com",
            Password = "Password123!",
            Role = "Requester"
        });

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.ProvisionUserAsync(new ProvisionDto
        {
            Username = "staff-one",
            Email = "staff@example.com",
            Password = "Password123!",
            Role = "Staff"
        }, "requester-one"));
    }

    [Fact]
    public async Task BootstrapManagerAsync_CreatesOnlyFirstManager()
    {
        var store = new InMemoryUserStore();
        var service = new AuthService(store);
        var first = new ProvisionDto
        {
            Username = "manager-one",
            Email = "manager@example.com",
            Password = "Password123!",
            Role = "Manager"
        };

        Assert.True(await service.BootstrapManagerAsync(first));
        Assert.False(await service.BootstrapManagerAsync(new ProvisionDto
        {
            Username = "manager-two",
            Email = "manager2@example.com",
            Password = "Password123!",
            Role = "Manager"
        }));
        Assert.Single(store.Audits);
    }

    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsUser()
    {
        var store = new InMemoryUserStore();
        var service = new AuthService(store);
        await service.RegisterAsync(new RegisterDto
        {
            Username = "login-user",
            Email = "login@example.com",
            Password = "Password123!",
            Role = "Requester"
        });

        var user = await service.LoginAsync(new LoginDto { Username = "login-user", Password = "Password123!" });

        Assert.Equal("login-user", user.Username);
    }

    [Fact]
    public async Task LoginAsync_WrongPassword_ThrowsUnauthorized()
    {
        var store = new InMemoryUserStore();
        var service = new AuthService(store);
        await service.RegisterAsync(new RegisterDto
        {
            Username = "login-user",
            Email = "login@example.com",
            Password = "Password123!",
            Role = "Requester"
        });

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.LoginAsync(
            new LoginDto { Username = "login-user", Password = "wrong-password" }));
    }

    private sealed class InMemoryUserStore : IUserStore
    {
        private readonly List<User> _users = [];
        private int _nextId = 1;

        public Task<User?> FindByUsernameAsync(string username) =>
            Task.FromResult(_users.FirstOrDefault(user => user.Username == username.Trim()));

        public Task<User?> FindByEmailAsync(string email) =>
            Task.FromResult(_users.FirstOrDefault(user =>
                string.Equals(user.Email, email.Trim(), StringComparison.OrdinalIgnoreCase)));

        public Task CreateUserAsync(User user)
        {
            user.Id = _nextId++;
            _users.Add(user);
            return Task.CompletedTask;
        }

        public List<AuditLog> Audits { get; } = [];

        public async Task CreateUserWithAuditAsync(User user, AuditLog auditLog)
        {
            await CreateUserAsync(user);
            auditLog.EntityId = user.Id;
            Audits.Add(auditLog);
        }

        public async Task<bool> TryCreateBootstrapManagerAsync(User user, AuditLog auditLog)
        {
            if (_users.Any(existing => existing.Role == "Manager")) return false;
            await CreateUserWithAuditAsync(user, auditLog);
            return true;
        }

        public Task<bool> ConfirmPasswordAsync(User user, string password) =>
            Task.FromResult(BCrypt.Net.BCrypt.Verify(password, user.PasswordHash));
    }
}

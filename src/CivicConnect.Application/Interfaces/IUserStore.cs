using CivicConnect.Domain.Entities;

namespace CivicConnect.Application.Interfaces;

/// <summary>
/// Store interface for user credential lookup and creation.
/// Enables swapping persistence strategy without affecting the authentication service.
/// </summary>
public interface IUserStore
{
    Task<User?> FindByUsernameAsync(string username);
    Task<User?> FindByEmailAsync(string email);
    Task CreateUserAsync(User user);
    Task CreateUserWithAuditAsync(User user, AuditLog auditLog);
    Task<bool> TryCreateBootstrapManagerAsync(User user, AuditLog auditLog);
    Task<bool> ConfirmPasswordAsync(User user, string password);
}

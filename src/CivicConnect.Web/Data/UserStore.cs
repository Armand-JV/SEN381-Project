using BCrypt.Net;
using CivicConnect.Application.Interfaces;
using CivicConnect.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace CivicConnect.Web.Data;

/// <summary>
/// EF Core-backed implementation of <see cref="IUserStore"/>.
/// </summary>
public class UserStore(CivicConnectDbContext context) : IUserStore
{
    private readonly CivicConnectDbContext _context = context;

    public Task<User?> FindByUsernameAsync(string username) =>
        _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Username == username.Trim());

    public Task<User?> FindByEmailAsync(string email)
    {
        // StringComparison overloads cannot be translated to SQL; compare normalized values.
        var normalizedEmail = email.Trim().ToLowerInvariant();
        return _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == normalizedEmail);
    }

    public async Task CreateUserAsync(User user)
    {
        _context.Users.Add(user);
        await SaveChangesAsync();
    }

    private async Task SaveChangesAsync()
    {
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            throw new InvalidOperationException("An account with that username or email already exists.", ex);
        }
    }

    public async Task CreateUserWithAuditAsync(User user, AuditLog auditLog)
    {
        if (_context.Database.IsNpgsql())
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();
            await SaveUserAndAuditAsync(user, auditLog);
            await transaction.CommitAsync();
            return;
        }

        await SaveUserAndAuditAsync(user, auditLog);
    }

    private async Task SaveUserAndAuditAsync(User user, AuditLog auditLog)
    {
        _context.Users.Add(user);
        await SaveChangesAsync();

        auditLog.EntityId = user.Id;
        _context.AuditLogs.Add(auditLog);
        await SaveChangesAsync();
    }

    public async Task<bool> TryCreateBootstrapManagerAsync(User user, AuditLog auditLog)
    {
        if (_context.Database.IsNpgsql())
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();
            // Serialize first-Manager checks across application instances until commit.
            await _context.Database.ExecuteSqlRawAsync("SELECT pg_advisory_xact_lock(1129270853)");
            var created = await TryCreateBootstrapManagerCoreAsync(user, auditLog);
            if (created)
            {
                await transaction.CommitAsync();
            }

            return created;
        }

        return await TryCreateBootstrapManagerCoreAsync(user, auditLog);
    }

    private async Task<bool> TryCreateBootstrapManagerCoreAsync(User user, AuditLog auditLog)
    {
        if (await _context.Users.AnyAsync(existing => existing.Role == "Manager"))
        {
            return false;
        }

        await SaveUserAndAuditAsync(user, auditLog);
        return true;
    }

    public Task<bool> ConfirmPasswordAsync(User user, string password) =>
        Task.FromResult(BCrypt.Net.BCrypt.Verify(password, user.PasswordHash));
}

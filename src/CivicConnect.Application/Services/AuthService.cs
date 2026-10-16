using System.Text.RegularExpressions;
using BCrypt.Net;
using CivicConnect.Application.Interfaces;
using CivicConnect.Domain.Entities;

namespace CivicConnect.Application.Services;

public sealed class RegisterDto
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}

public sealed class ProvisionDto
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}

public sealed class ProvisionedUserDto
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}

public sealed class LoginDto
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public sealed class TokenResult
{
    public string UserId { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
}

/// <summary>
/// Handles user registration, privileged provisioning, and login using BCrypt-hashed passwords.
/// JWT issuance lives in the Web layer; this service returns the authenticated <see cref="User"/>.
/// </summary>
public class AuthService
{
    private readonly IUserStore _userStore;

    public AuthService(IUserStore userStore)
    {
        _userStore = userStore;
    }

    public static string ValidateUsername(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
            return "Username is required.";
        if (username.Length < 3 || username.Length > 50)
            return "Username must be between 3 and 50 characters.";
        if (!Regex.IsMatch(username, @"^[a-zA-Z0-9_-]+$"))
            return "Username may only contain letters, numbers, underscores, and hyphens.";
        return string.Empty;
    }

    public static string ValidateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return "Email is required.";
        var regex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        if (!regex.IsMatch(email))
            return "Email format is invalid.";
        if (email.Length > 256)
            return "Email must be 256 characters or fewer.";
        return string.Empty;
    }

    public static string ValidatePassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            return "Password is required.";
        if (password.Length < 8)
            return "Password must be at least 8 characters.";
        if (System.Text.Encoding.UTF8.GetByteCount(password) > 72)
            return "Password must be 72 UTF-8 bytes or fewer.";
        return string.Empty;
    }

    public static string ValidateRole(string role)
    {
        if (string.IsNullOrWhiteSpace(role))
            return "Role is required.";
        if (!string.Equals(role.Trim(), "Requester", StringComparison.OrdinalIgnoreCase))
            return "New accounts can only be registered as Requester.";
        return string.Empty;
    }

    public static string ValidateProvisionRole(string role)
    {
        if (string.IsNullOrWhiteSpace(role))
            return "Role is required.";
        if (!string.Equals(role.Trim(), "Staff", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(role.Trim(), "Manager", StringComparison.OrdinalIgnoreCase))
            return "New privileged accounts may only be Staff or Manager.";
        return string.Empty;
    }

    public async Task<User> RegisterAsync(RegisterDto dto)
    {
        ValidateCredentials(dto.Username, dto.Email, dto.Password, ValidateRole(dto.Role));
        await EnsureUniqueAsync(dto.Username, dto.Email);

        var user = BuildUser(dto.Username, dto.Email, dto.Password, "Requester");
        await _userStore.CreateUserAsync(user);
        return user;
    }

    /// <summary>
    /// Creates a Staff or Manager account with an audit record of the provisioning Manager.
    /// </summary>
    public async Task<User> ProvisionUserAsync(ProvisionDto dto, string actor)
    {
        if (string.IsNullOrWhiteSpace(actor))
            throw new ArgumentException("An authenticated provisioning actor is required.");

        var manager = await _userStore.FindByUsernameAsync(actor);
        if (manager?.Role != "Manager")
            throw new UnauthorizedAccessException("Only an existing Manager may provision accounts.");

        ValidateCredentials(dto.Username, dto.Email, dto.Password, ValidateProvisionRole(dto.Role));
        await EnsureUniqueAsync(dto.Username, dto.Email);

        var role = NormalizePrivilegedRole(dto.Role);
        var user = BuildUser(dto.Username, dto.Email, dto.Password, role);
        var audit = BuildProvisionAudit(actor, role);
        await _userStore.CreateUserWithAuditAsync(user, audit);
        return user;
    }

    /// <summary>
    /// One-time deployment bootstrap for the first Manager account.
    /// Returns false when a Manager already exists; never resets existing credentials.
    /// </summary>
    public async Task<bool> BootstrapManagerAsync(ProvisionDto dto)
    {
        if (!string.Equals(dto.Role?.Trim(), "Manager", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Bootstrap account must have Manager role.");

        ValidateCredentials(dto.Username, dto.Email, dto.Password, string.Empty);
        var user = BuildUser(dto.Username, dto.Email, dto.Password, "Manager");
        var audit = BuildProvisionAudit("system:bootstrap", "Manager");
        return await _userStore.TryCreateBootstrapManagerAsync(user, audit);
    }

    public async Task<User> LoginAsync(LoginDto dto)
    {
        var user = await _userStore.FindByUsernameAsync(dto.Username);
        if (user == null || !await _userStore.ConfirmPasswordAsync(user, dto.Password))
            throw new UnauthorizedAccessException("Invalid username or password.");

        return user;
    }

    private static void ValidateCredentials(string username, string email, string password, string roleError)
    {
        var errors = new List<string>
        {
            ValidateUsername(username),
            ValidateEmail(email),
            ValidatePassword(password),
            roleError
        };
        errors.RemoveAll(string.IsNullOrEmpty);

        if (errors.Count > 0)
            throw new ArgumentException(string.Join(" ", errors));
    }

    private async Task EnsureUniqueAsync(string username, string email)
    {
        var normalizedUsername = username.Trim();
        var normalizedEmail = email.Trim().ToLowerInvariant();

        if (await _userStore.FindByUsernameAsync(normalizedUsername) != null)
            throw new InvalidOperationException("A user with that username already exists.");

        if (await _userStore.FindByEmailAsync(normalizedEmail) != null)
            throw new InvalidOperationException("An account with that email already exists.");
    }

    private static User BuildUser(string username, string email, string password, string role)
    {
        var salt = BCrypt.Net.BCrypt.GenerateSalt();
        var hash = BCrypt.Net.BCrypt.HashPassword(password, salt);

        return new User
        {
            Username = username.Trim(),
            Email = email.Trim().ToLowerInvariant(),
            PasswordHash = hash,
            PasswordSalt = salt,
            Role = role,
            CreatedAt = DateTime.UtcNow
        };
    }

    private static string NormalizePrivilegedRole(string role) =>
        string.Equals(role.Trim(), "Staff", StringComparison.OrdinalIgnoreCase) ? "Staff" : "Manager";

    private static AuditLog BuildProvisionAudit(string actor, string role) => new()
    {
        Actor = actor,
        Action = "UserProvisioned",
        EntityType = nameof(User),
        EntityId = 0, // Set by the store after the user ID is generated.
        AfterSummary = $"Role:{role}",
        Timestamp = DateTime.UtcNow,
        CorrelationId = Guid.NewGuid().ToString()
    };
}

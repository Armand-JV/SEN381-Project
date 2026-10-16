using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CivicConnect.Domain.Entities;

/// <summary>
/// Represents an application user with login credentials.
/// Roles match the claims-based values used throughout the permission system.
/// </summary>
public class User
{
    public int Id { get; set; }

    [MaxLength(50)]
    public string Username { get; set; } = string.Empty;

    [MaxLength(256)]
    public string Email { get; set; } = string.Empty;

    [JsonIgnore]
    public string PasswordHash { get; set; } = string.Empty;

    [JsonIgnore]
    public string PasswordSalt { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    public static readonly string[] ValidRoles = { "Requester", "Staff", "Manager" };
}

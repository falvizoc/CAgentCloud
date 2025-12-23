namespace CobranzaCloud.Core.Entities;

/// <summary>
/// Represents a user in the system
/// </summary>
public class User
{
    public Guid Id { get; set; }
    public required string Email { get; set; }
    public required string Nombre { get; set; }
    public string? PasswordHash { get; set; }
    public UserRole Role { get; set; } = UserRole.Viewer;

    // OAuth
    public string? GoogleId { get; set; }
    public string? MicrosoftId { get; set; }
    public AuthProvider? AuthProvider { get; set; }

    // Status
    public bool EmailConfirmed { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? LastLoginAt { get; set; }
    public int FailedLoginAttempts { get; set; }
    public DateTime? LockoutEnd { get; set; }

    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Organization (required - multi-tenant)
    public Guid OrganizationId { get; set; }
    public Organization Organization { get; set; } = null!;

    // Refresh tokens
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}

public enum UserRole
{
    Viewer,
    Collector,
    Manager,
    Admin,
    Owner
}

public enum AuthProvider
{
    Email,
    Google,
    Microsoft
}

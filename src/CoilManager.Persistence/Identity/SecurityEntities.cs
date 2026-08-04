using Microsoft.AspNetCore.Identity;

namespace CoilManager.Persistence.Identity;

public sealed class ApplicationUser : IdentityUser<Guid>
{
    public string DisplayName { get; set; } = string.Empty;
    public string? JobTitle { get; set; }
    public bool IsActive { get; set; } = true;
    public bool MustChangePassword { get; set; }
    public DateTimeOffset? LastLoginAtUtc { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; } = DateTimeOffset.UtcNow;
    public Guid? CreatedBy { get; set; }
    public DateTimeOffset? ModifiedAtUtc { get; set; }
    public Guid? ModifiedBy { get; set; }
}

public sealed class ApplicationRole : IdentityRole<Guid>
{
    public string? Description { get; set; }
    public bool IsSystem { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; } = DateTimeOffset.UtcNow;
}

public sealed class Permission
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Module { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ICollection<RolePermission> Roles { get; set; } = [];
}
public sealed class RolePermission
{
    public Guid RoleId { get; set; }
    public ApplicationRole Role { get; set; } = null!;
    public Guid PermissionId { get; set; }
    public Permission Permission { get; set; } = null!;
}
public sealed class LoginOtp
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;
    public string CodeHash { get; set; } = string.Empty;
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset ExpiresAtUtc { get; set; }
    public DateTimeOffset ResendAvailableAtUtc { get; set; }
    public int FailedAttempts { get; set; }
    public bool IsUsed { get; set; }
    public bool RememberMe { get; set; }
}
public sealed class UserSession
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;
    public string RefreshTokenHash { get; set; } = string.Empty;
    public DateTimeOffset RefreshTokenExpiresAtUtc { get; set; }
    public DateTimeOffset LoginAtUtc { get; set; }
    public DateTimeOffset LastActivityAtUtc { get; set; }
    public string? Device { get; set; }
    public string? Browser { get; set; }
    public string? IpAddress { get; set; }
    public DateTimeOffset? RevokedAtUtc { get; set; }
    public string? ReplacedByTokenHash { get; set; }
}
public sealed class AuditLog
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTimeOffset TimestampUtc { get; set; } = DateTimeOffset.UtcNow;
    public Guid? UserId { get; set; }
    public string? UserEmail { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string Outcome { get; set; } = string.Empty;
    public string? IpAddress { get; set; }
    public string? CorrelationId { get; set; }
    public string? Details { get; set; }
}
public sealed class CompanyProfile
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string CompanyName { get; set; } = "Coil Manager";
    public string? LogoUrl { get; set; }
    public string? Address { get; set; }
    public string? GstNumber { get; set; }
    public string? Pan { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Website { get; set; }
    public string DefaultCurrency { get; set; } = "INR";
    public string DefaultTimezone { get; set; } = "Asia/Kolkata";
    public string? SmtpHost { get; set; }
    public int? SmtpPort { get; set; }
    public string? SmtpUserName { get; set; }
    public string? SmtpPasswordEncrypted { get; set; }
    public bool SmtpUseTls { get; set; } = true;
    public string BarcodePrefix { get; set; } = "CM";
    public string? PackingSlipFooter { get; set; }
    public string? ReportHeader { get; set; }
    public string? ReportFooter { get; set; }
    public DateTimeOffset ModifiedAtUtc { get; set; } = DateTimeOffset.UtcNow;
    public Guid? ModifiedBy { get; set; }
}

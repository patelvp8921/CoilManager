using MediatR;

namespace CoilManager.Application.Security;

public sealed record LoginCommand(string Email, string Password, bool RememberMe) : IRequest<TokenResponseDto>;
public sealed record VerifyOtpCommand(Guid ChallengeId, string Code, string? DeviceName) : IRequest<TokenResponseDto>;
public sealed record RefreshTokenCommand(string RefreshToken) : IRequest<TokenResponseDto>;
public sealed record ForgotPasswordCommand(string Email) : IRequest;
public sealed record ResetPasswordCommand(string Email, string Token, string NewPassword) : IRequest;

public sealed record LoginChallengeDto(Guid ChallengeId, string MaskedEmail, DateTimeOffset ExpiresAtUtc, int ResendAfterSeconds);
public sealed record TokenResponseDto(string AccessToken, DateTimeOffset AccessTokenExpiresAtUtc, string RefreshToken,
    DateTimeOffset RefreshTokenExpiresAtUtc, UserIdentityDto User);
public sealed record UserIdentityDto(Guid Id, string Email, string DisplayName, bool MustChangePassword,
    IReadOnlyCollection<string> Roles, IReadOnlyCollection<string> Permissions);
public sealed record UserSummaryDto(Guid Id, string Email, string DisplayName, bool IsActive, bool IsLocked,
    DateTimeOffset? LastLoginAtUtc, DateTimeOffset CreatedAtUtc, IReadOnlyCollection<string> Roles);
public sealed record UserDetailDto(Guid Id, string Email, string DisplayName, string? PhoneNumber, string? JobTitle,
    bool IsActive, bool MustChangePassword, bool IsLocked, DateTimeOffset? LastLoginAtUtc,
    DateTimeOffset CreatedAtUtc, string? CreatedBy, DateTimeOffset? ModifiedAtUtc, string? ModifiedBy,
    IReadOnlyCollection<string> Roles, IReadOnlyCollection<string> Permissions);
public sealed record CreateUserRequest(string Email, string DisplayName, string? PhoneNumber, string? JobTitle,
    string TemporaryPassword, bool MustChangePassword, IReadOnlyCollection<string> Roles);
public sealed record UpdateUserRequest(string DisplayName, string? PhoneNumber, string? JobTitle,
    IReadOnlyCollection<string> Roles);
public sealed record RoleDto(Guid Id, string Name, string? Description, bool IsSystem,
    IReadOnlyCollection<string> Permissions, int AssignedUsers);
public sealed record SaveRoleRequest(string Name, string? Description);
public sealed record SessionDto(Guid Id, Guid UserId, string UserEmail, string? Device, string? Browser,
    string? IpAddress, DateTimeOffset LoginAtUtc, DateTimeOffset LastActivityAtUtc, bool IsCurrent, bool IsRevoked);
public sealed record AuditLogDto(Guid Id, DateTimeOffset TimestampUtc, Guid? UserId, string? UserEmail,
    string Category, string Action, string Outcome, string? IpAddress, string? Details);
public sealed record AuditSearchRequest(string? Search, DateTimeOffset? FromUtc, DateTimeOffset? ToUtc,
    Guid? UserId, string? Category, int Page = 1, int PageSize = 50);
public sealed record PagedResult<T>(IReadOnlyCollection<T> Items, int TotalCount, int Page, int PageSize);
public sealed record CompanyProfileDto(Guid Id, string CompanyName, string? LogoUrl, string? Address,
    string? GstNumber, string? Pan, string? Phone, string? Email, string? Website, string DefaultCurrency,
    string DefaultTimezone, string? SmtpHost, int? SmtpPort, string? SmtpUserName, bool SmtpUseTls,
    string BarcodePrefix, string? PackingSlipFooter, string? ReportHeader, string? ReportFooter);
public sealed record UpdateCompanyProfileRequest(string CompanyName, string? LogoUrl, string? Address,
    string? GstNumber, string? Pan, string? Phone, string? Email, string? Website, string DefaultCurrency,
    string DefaultTimezone, string? SmtpHost, int? SmtpPort, string? SmtpUserName, string? SmtpPassword,
    bool SmtpUseTls, string BarcodePrefix, string? PackingSlipFooter, string? ReportHeader, string? ReportFooter);
public sealed record SecuritySettingsDto(int AccessTokenMinutes, int RefreshTokenDays, int OtpExpiryMinutes,
    int OtpMaximumAttempts, int OtpResendSeconds, int MaximumFailedLoginAttempts, int LockoutMinutes);

public interface ISecurityPlatformService
{
    Task<TokenResponseDto> LoginAsync(string email, string password, bool rememberMe, string? device, string? browser, string? ipAddress, CancellationToken ct);
    Task<LoginChallengeDto> ResendOtpAsync(Guid challengeId, string? ipAddress, CancellationToken ct);
    Task<TokenResponseDto> VerifyOtpAsync(Guid challengeId, string code, string? device, string? browser, string? ipAddress, CancellationToken ct);
    Task<TokenResponseDto> RefreshAsync(string refreshToken, string? ipAddress, CancellationToken ct);
    Task LogoutAsync(Guid sessionId, Guid userId, CancellationToken ct);
    Task LogoutAllAsync(Guid userId, CancellationToken ct);
    Task ForgotPasswordAsync(string email, CancellationToken ct);
    Task ResetPasswordAsync(string email, string token, string newPassword, CancellationToken ct);
    Task<PagedResult<UserSummaryDto>> GetUsersAsync(string? search, bool? active, int page, int pageSize, CancellationToken ct);
    Task<UserDetailDto> GetUserAsync(Guid id, CancellationToken ct);
    Task<Guid> CreateUserAsync(CreateUserRequest request, Guid actorId, CancellationToken ct);
    Task UpdateUserAsync(Guid id, UpdateUserRequest request, Guid actorId, CancellationToken ct);
    Task SetUserActiveAsync(Guid id, bool active, Guid actorId, CancellationToken ct);
    Task UnlockUserAsync(Guid id, Guid actorId, CancellationToken ct);
    Task AdminResetPasswordAsync(Guid id, string temporaryPassword, Guid actorId, CancellationToken ct);
    Task<IReadOnlyCollection<RoleDto>> GetRolesAsync(CancellationToken ct);
    Task<Guid> CreateRoleAsync(SaveRoleRequest request, Guid actorId, CancellationToken ct);
    Task UpdateRoleAsync(Guid id, SaveRoleRequest request, Guid actorId, CancellationToken ct);
    Task DeleteRoleAsync(Guid id, Guid actorId, CancellationToken ct);
    Task<IReadOnlyCollection<string>> GetPermissionsAsync(CancellationToken ct);
    Task SetRolePermissionsAsync(Guid id, IReadOnlyCollection<string> permissions, Guid actorId, CancellationToken ct);
    Task<IReadOnlyCollection<SessionDto>> GetSessionsAsync(Guid? userId, Guid currentSessionId, CancellationToken ct);
    Task RevokeSessionAsync(Guid id, Guid actorId, CancellationToken ct);
    Task<PagedResult<AuditLogDto>> SearchAuditAsync(AuditSearchRequest request, CancellationToken ct);
    Task<CompanyProfileDto> GetCompanyAsync(CancellationToken ct);
    Task UpdateCompanyAsync(UpdateCompanyProfileRequest request, Guid actorId, CancellationToken ct);
}

public interface IEmailSender
{
    Task SendAsync(string recipient, string subject, string htmlBody, CancellationToken ct);
}
public interface ISecurityTokenService
{
    (string Token, DateTimeOffset ExpiresAtUtc) CreateAccessToken(UserIdentityDto user, Guid sessionId);
    string CreateRefreshToken();
    string HashSecret(string value);
    string HashOtp(Guid challengeId, string code);
}

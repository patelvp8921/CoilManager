using System.Security.Cryptography;
using CoilManager.Application.Security;
using CoilManager.Shared.Exceptions;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace CoilManager.Persistence.Identity;

public sealed class SecurityPlatformService(
    ApplicationDbContext db, UserManager<ApplicationUser> users, RoleManager<ApplicationRole> roles,
    ISecurityTokenService tokens, IEmailSender emailSender, IConfiguration configuration,
    IDataProtectionProvider dataProtection) : ISecurityPlatformService
{
    private readonly IDataProtector _smtpProtector = dataProtection.CreateProtector("CoilManager.Company.Smtp.v1");
    private int OtpMinutes => configuration.GetValue("Security:OtpExpiryMinutes", 5);
    private int OtpAttempts => configuration.GetValue("Security:OtpMaximumAttempts", 5);
    private int ResendSeconds => configuration.GetValue("Security:OtpResendSeconds", 60);
    private int RefreshDays => configuration.GetValue("JwtSettings:RefreshTokenDays", 30);

    public async Task<TokenResponseDto> LoginAsync(string email, string password, bool rememberMe, string? device, string? browser, string? ip, CancellationToken ct)
    {
        ApplicationUser? user = await users.FindByEmailAsync(email.Trim());
        if (user is null || !user.IsActive)
        {
            await Audit(null, email, "Authentication", "Login", "Failed", ip, "Invalid credentials or inactive account.", ct);
            throw new UnauthorizedException("Invalid email or password.");
        }
        if (await users.IsLockedOutAsync(user)) throw new UnauthorizedException("Account is temporarily locked.");
        if (!await users.CheckPasswordAsync(user, password))
        {
            await users.AccessFailedAsync(user);
            await Audit(user.Id, user.Email, "Authentication", "Failed Login", "Failed", ip, null, ct);
            throw new UnauthorizedException("Invalid email or password.");
        }
        await users.ResetAccessFailedCountAsync(user);
        string refresh = tokens.CreateRefreshToken();
        DateTimeOffset now = DateTimeOffset.UtcNow;
        UserSession session = new() { UserId = user.Id, RefreshTokenHash = tokens.HashSecret(refresh),
            RefreshTokenExpiresAtUtc = now.AddDays(rememberMe ? RefreshDays : 1), LoginAtUtc = now,
            LastActivityAtUtc = now, Device = device, Browser = browser, IpAddress = ip };
        db.UserSessions.Add(session); user.LastLoginAtUtc = now;
        UserIdentityDto identity = await Identity(user, ct);
        (string access, DateTimeOffset expires) = tokens.CreateAccessToken(identity, session.Id);
        await Audit(user.Id, user.Email, "Authentication", "Login", "Success", ip, null, ct, false);
        await db.SaveChangesAsync(ct);
        return new(access, expires, refresh, session.RefreshTokenExpiresAtUtc, identity);
    }

    public async Task<LoginChallengeDto> ResendOtpAsync(Guid challengeId, string? ip, CancellationToken ct)
    {
        LoginOtp previous = await db.LoginOtps.Include(x => x.User).SingleOrDefaultAsync(x => x.Id == challengeId, ct)
            ?? throw new NotFoundException("Login challenge not found.");
        if (previous.ResendAvailableAtUtc > DateTimeOffset.UtcNow) throw new BusinessRuleException("Please wait before requesting another code.");
        previous.IsUsed = true;
        return await IssueOtp(previous.User, previous.RememberMe, ip, ct);
    }

    private async Task<LoginChallengeDto> IssueOtp(ApplicationUser user, bool rememberMe, string? ip, CancellationToken ct)
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        await db.LoginOtps.Where(x => x.UserId == user.Id && !x.IsUsed).ExecuteUpdateAsync(x => x.SetProperty(p => p.IsUsed, true), ct);
        string code = RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");
        LoginOtp otp = new() { UserId = user.Id, CreatedAtUtc = now, ExpiresAtUtc = now.AddMinutes(OtpMinutes),
            ResendAvailableAtUtc = now.AddSeconds(ResendSeconds), RememberMe = rememberMe };
        otp.CodeHash = tokens.HashOtp(otp.Id, code);
        db.LoginOtps.Add(otp);
        await Audit(user.Id, user.Email, "Authentication", "OTP Generated", "Success", ip, null, ct, false);
        await db.SaveChangesAsync(ct);
        await emailSender.SendAsync(user.Email!, "Your Coil Manager verification code",
            $"<p>Your verification code is <strong>{code}</strong>. It expires in {OtpMinutes} minutes.</p>", ct);
        return new LoginChallengeDto(otp.Id, MaskEmail(user.Email!), otp.ExpiresAtUtc, ResendSeconds);
    }

    public async Task<TokenResponseDto> VerifyOtpAsync(Guid id, string code, string? device, string? browser, string? ip, CancellationToken ct)
    {
        LoginOtp otp = await db.LoginOtps.Include(x => x.User).SingleOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new UnauthorizedException("Invalid or expired verification code.");
        if (otp.IsUsed || otp.ExpiresAtUtc <= DateTimeOffset.UtcNow || otp.FailedAttempts >= OtpAttempts)
            throw new UnauthorizedException("Invalid or expired verification code.");
        if (!CryptographicOperations.FixedTimeEquals(Convert.FromHexString(otp.CodeHash), Convert.FromHexString(tokens.HashOtp(id, code))))
        {
            otp.FailedAttempts++;
            await db.SaveChangesAsync(ct);
            throw new UnauthorizedException("Invalid or expired verification code.");
        }
        otp.IsUsed = true;
        otp.User.LastLoginAtUtc = DateTimeOffset.UtcNow;
        string refresh = tokens.CreateRefreshToken();
        UserSession session = new() { UserId = otp.UserId, RefreshTokenHash = tokens.HashSecret(refresh),
            RefreshTokenExpiresAtUtc = DateTimeOffset.UtcNow.AddDays(RefreshDays), LoginAtUtc = DateTimeOffset.UtcNow,
            LastActivityAtUtc = DateTimeOffset.UtcNow, Device = device, Browser = browser, IpAddress = ip };
        db.UserSessions.Add(session);
        UserIdentityDto identity = await Identity(otp.User, ct);
        (string access, DateTimeOffset expires) = tokens.CreateAccessToken(identity, session.Id);
        await Audit(otp.UserId, otp.User.Email, "Authentication", "OTP Verified", "Success", ip, null, ct, false);
        await db.SaveChangesAsync(ct);
        return new(access, expires, refresh, session.RefreshTokenExpiresAtUtc, identity);
    }

    public async Task<TokenResponseDto> RefreshAsync(string refreshToken, string? ip, CancellationToken ct)
    {
        string hash = tokens.HashSecret(refreshToken);
        UserSession session = await db.UserSessions.Include(x => x.User).SingleOrDefaultAsync(x => x.RefreshTokenHash == hash, ct)
            ?? throw new UnauthorizedException("Invalid refresh token.");
        if (session.RevokedAtUtc is not null || session.RefreshTokenExpiresAtUtc <= DateTimeOffset.UtcNow || !session.User.IsActive)
            throw new UnauthorizedException("Session has expired or was revoked.");
        string rotated = tokens.CreateRefreshToken();
        string rotatedHash = tokens.HashSecret(rotated);
        session.RevokedAtUtc = DateTimeOffset.UtcNow;
        session.ReplacedByTokenHash = rotatedHash;
        UserSession replacement = new() { UserId = session.UserId, RefreshTokenHash = rotatedHash,
            RefreshTokenExpiresAtUtc = DateTimeOffset.UtcNow.AddDays(RefreshDays), LoginAtUtc = session.LoginAtUtc,
            LastActivityAtUtc = DateTimeOffset.UtcNow, Device = session.Device, Browser = session.Browser, IpAddress = ip ?? session.IpAddress };
        db.UserSessions.Add(replacement);
        UserIdentityDto identity = await Identity(session.User, ct);
        (string access, DateTimeOffset expires) = tokens.CreateAccessToken(identity, replacement.Id);
        await db.SaveChangesAsync(ct);
        return new(access, expires, rotated, replacement.RefreshTokenExpiresAtUtc, identity);
    }

    public async Task LogoutAsync(Guid sessionId, Guid userId, CancellationToken ct)
    { UserSession? x = await db.UserSessions.SingleOrDefaultAsync(s => s.Id == sessionId && s.UserId == userId, ct); if (x is not null) x.RevokedAtUtc = DateTimeOffset.UtcNow; await Audit(userId, null, "Authentication", "Logout", "Success", null, null, ct); }
    public async Task LogoutAllAsync(Guid userId, CancellationToken ct)
    { await db.UserSessions.Where(x => x.UserId == userId && x.RevokedAtUtc == null).ExecuteUpdateAsync(x => x.SetProperty(p => p.RevokedAtUtc, DateTimeOffset.UtcNow), ct); await Audit(userId, null, "Session", "Logout All Devices", "Success", null, null, ct); }

    public async Task ForgotPasswordAsync(string email, CancellationToken ct)
    {
        ApplicationUser? user = await users.FindByEmailAsync(email); if (user is null || !user.IsActive) return;
        string token = await users.GeneratePasswordResetTokenAsync(user);
        await emailSender.SendAsync(user.Email!, "Reset your Coil Manager password", $"<p>Use this reset token: {System.Net.WebUtility.HtmlEncode(token)}</p>", ct);
        await Audit(user.Id, user.Email, "Security", "Password Reset Requested", "Success", null, null, ct);
    }
    public async Task ResetPasswordAsync(string email, string token, string password, CancellationToken ct)
    {
        ApplicationUser user = await users.FindByEmailAsync(email) ?? throw new UnauthorizedException("Invalid password reset request.");
        IdentityResult result = await users.ResetPasswordAsync(user, token, password); Ensure(result); user.MustChangePassword = false;
        await LogoutAllAsync(user.Id, ct); await Audit(user.Id, user.Email, "Security", "Password Reset", "Success", null, null, ct);
    }

    public async Task<PagedResult<UserSummaryDto>> GetUsersAsync(string? search, bool? active, int page, int size, CancellationToken ct)
    {
        IQueryable<ApplicationUser> query = users.Users.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search)) query = query.Where(x => x.Email!.Contains(search) || x.DisplayName.Contains(search));
        if (active.HasValue) query = query.Where(x => x.IsActive == active);
        int count = await query.CountAsync(ct); List<ApplicationUser> items = await query.OrderBy(x => x.DisplayName).Skip((page - 1) * size).Take(size).ToListAsync(ct);
        List<UserSummaryDto> result = []; foreach (ApplicationUser x in items) result.Add(new(x.Id, x.Email!, x.DisplayName, x.IsActive,
            x.LockoutEnd > DateTimeOffset.UtcNow, x.LastLoginAtUtc, x.CreatedAtUtc, (await users.GetRolesAsync(x)).ToArray()));
        return new(result, count, page, size);
    }
    public async Task<UserDetailDto> GetUserAsync(Guid id, CancellationToken ct)
    {
        ApplicationUser user = await users.Users.SingleOrDefaultAsync(x => x.Id == id, ct) ?? throw new NotFoundException("User not found.");
        UserIdentityDto identity = await Identity(user, ct);
        return new(user.Id, user.Email!, user.DisplayName, user.PhoneNumber, user.JobTitle, user.IsActive, user.MustChangePassword,
            user.LockoutEnd > DateTimeOffset.UtcNow, user.LastLoginAtUtc, user.CreatedAtUtc, user.CreatedBy?.ToString(), user.ModifiedAtUtc,
            user.ModifiedBy?.ToString(), identity.Roles, identity.Permissions);
    }
    public async Task<Guid> CreateUserAsync(CreateUserRequest request, Guid actor, CancellationToken ct)
    {
        ApplicationUser user = new() { Id = Guid.NewGuid(), UserName = request.Email, Email = request.Email, EmailConfirmed = true,
            DisplayName = request.DisplayName, PhoneNumber = request.PhoneNumber, JobTitle = request.JobTitle,
            MustChangePassword = request.MustChangePassword, CreatedBy = actor };
        Ensure(await users.CreateAsync(user, request.TemporaryPassword)); Ensure(await users.AddToRolesAsync(user, request.Roles));
        await Audit(actor, null, "Administration", "User Created", "Success", null, user.Email, ct); return user.Id;
    }
    public async Task UpdateUserAsync(Guid id, UpdateUserRequest request, Guid actor, CancellationToken ct)
    {
        ApplicationUser user = await FindUser(id); user.DisplayName = request.DisplayName; user.PhoneNumber = request.PhoneNumber; user.JobTitle = request.JobTitle; user.ModifiedBy = actor; user.ModifiedAtUtc = DateTimeOffset.UtcNow;
        Ensure(await users.UpdateAsync(user)); string[] current = (await users.GetRolesAsync(user)).ToArray(); Ensure(await users.RemoveFromRolesAsync(user, current.Except(request.Roles))); Ensure(await users.AddToRolesAsync(user, request.Roles.Except(current)));
        await Audit(actor, null, "Administration", "User Updated", "Success", null, user.Email, ct);
    }
    public async Task SetUserActiveAsync(Guid id, bool active, Guid actor, CancellationToken ct) { ApplicationUser user = await FindUser(id); user.IsActive = active; Ensure(await users.UpdateAsync(user)); if (!active) await LogoutAllAsync(id, ct); await Audit(actor, null, "Administration", active ? "User Activated" : "User Deactivated", "Success", null, user.Email, ct); }
    public async Task UnlockUserAsync(Guid id, Guid actor, CancellationToken ct) { ApplicationUser user = await FindUser(id); await users.SetLockoutEndDateAsync(user, null); await users.ResetAccessFailedCountAsync(user); await Audit(actor, null, "Security", "Account Unlocked", "Success", null, user.Email, ct); }
    public async Task AdminResetPasswordAsync(Guid id, string password, Guid actor, CancellationToken ct) { ApplicationUser user = await FindUser(id); string token = await users.GeneratePasswordResetTokenAsync(user); Ensure(await users.ResetPasswordAsync(user, token, password)); user.MustChangePassword = true; await LogoutAllAsync(id, ct); await Audit(actor, null, "Security", "Admin Password Reset", "Success", null, user.Email, ct); }

    public async Task<IReadOnlyCollection<RoleDto>> GetRolesAsync(CancellationToken ct)
    {
        List<RoleDto> result = []; foreach (ApplicationRole role in await roles.Roles.OrderBy(x => x.Name).ToListAsync(ct))
        { string[] permissions = await db.RolePermissions.Where(x => x.RoleId == role.Id).Select(x => x.Permission.Name).ToArrayAsync(ct); int count = await db.UserRoles.CountAsync(x => x.RoleId == role.Id, ct); result.Add(new(role.Id, role.Name!, role.Description, role.IsSystem, permissions, count)); } return result;
    }
    public async Task<Guid> CreateRoleAsync(SaveRoleRequest request, Guid actor, CancellationToken ct) { ApplicationRole role = new() { Id = Guid.NewGuid(), Name = request.Name, Description = request.Description }; Ensure(await roles.CreateAsync(role)); await Audit(actor, null, "Authorization", "Role Created", "Success", null, role.Name, ct); return role.Id; }
    public async Task UpdateRoleAsync(Guid id, SaveRoleRequest request, Guid actor, CancellationToken ct) { ApplicationRole role = await FindRole(id); role.Name = request.Name; role.Description = request.Description; Ensure(await roles.UpdateAsync(role)); await Audit(actor, null, "Authorization", "Role Updated", "Success", null, role.Name, ct); }
    public async Task DeleteRoleAsync(Guid id, Guid actor, CancellationToken ct) { ApplicationRole role = await FindRole(id); if (role.IsSystem) throw new BusinessRuleException("System roles cannot be deleted."); Ensure(await roles.DeleteAsync(role)); await Audit(actor, null, "Authorization", "Role Deleted", "Success", null, role.Name, ct); }
    public async Task<IReadOnlyCollection<string>> GetPermissionsAsync(CancellationToken ct) => await db.Permissions.OrderBy(x => x.Name).Select(x => x.Name).ToArrayAsync(ct);
    public async Task SetRolePermissionsAsync(Guid id, IReadOnlyCollection<string> names, Guid actor, CancellationToken ct)
    {
        _ = await FindRole(id); List<Permission> selected = await db.Permissions.Where(x => names.Contains(x.Name)).ToListAsync(ct);
        if (selected.Count != names.Distinct(StringComparer.OrdinalIgnoreCase).Count()) throw new ValidationException(["One or more permissions are invalid."]);
        await db.RolePermissions.Where(x => x.RoleId == id).ExecuteDeleteAsync(ct); db.RolePermissions.AddRange(selected.Select(x => new RolePermission { RoleId = id, PermissionId = x.Id }));
        await Audit(actor, null, "Authorization", "Permission Changed", "Success", null, $"Role {id}", ct);
    }
    public async Task<IReadOnlyCollection<SessionDto>> GetSessionsAsync(Guid? userId, Guid current, CancellationToken ct) => await db.UserSessions.AsNoTracking().Include(x => x.User).Where(x => !userId.HasValue || x.UserId == userId).OrderByDescending(x => x.LastActivityAtUtc).Select(x => new SessionDto(x.Id, x.UserId, x.User.Email!, x.Device, x.Browser, x.IpAddress, x.LoginAtUtc, x.LastActivityAtUtc, x.Id == current, x.RevokedAtUtc != null)).ToArrayAsync(ct);
    public async Task RevokeSessionAsync(Guid id, Guid actor, CancellationToken ct) { UserSession session = await db.UserSessions.SingleOrDefaultAsync(x => x.Id == id, ct) ?? throw new NotFoundException("Session not found."); session.RevokedAtUtc = DateTimeOffset.UtcNow; await Audit(actor, null, "Session", "Session Revoked", "Success", null, id.ToString(), ct); }
    public async Task<PagedResult<AuditLogDto>> SearchAuditAsync(AuditSearchRequest request, CancellationToken ct)
    {
        IQueryable<AuditLog> query = db.AuditLogs.AsNoTracking(); if (request.FromUtc.HasValue) query = query.Where(x => x.TimestampUtc >= request.FromUtc); if (request.ToUtc.HasValue) query = query.Where(x => x.TimestampUtc <= request.ToUtc); if (request.UserId.HasValue) query = query.Where(x => x.UserId == request.UserId); if (!string.IsNullOrWhiteSpace(request.Category)) query = query.Where(x => x.Category == request.Category); if (!string.IsNullOrWhiteSpace(request.Search)) query = query.Where(x => x.Action.Contains(request.Search) || (x.UserEmail != null && x.UserEmail.Contains(request.Search)));
        int count = await query.CountAsync(ct); AuditLogDto[] items = await query.OrderByDescending(x => x.TimestampUtc).Skip((request.Page - 1) * request.PageSize).Take(request.PageSize).Select(x => new AuditLogDto(x.Id, x.TimestampUtc, x.UserId, x.UserEmail, x.Category, x.Action, x.Outcome, x.IpAddress, x.Details)).ToArrayAsync(ct); return new(items, count, request.Page, request.PageSize);
    }
    public async Task<CompanyProfileDto> GetCompanyAsync(CancellationToken ct) { CompanyProfile company = await db.CompanyProfiles.AsNoTracking().SingleOrDefaultAsync(ct) ?? new(); return Map(company); }
    public async Task UpdateCompanyAsync(UpdateCompanyProfileRequest r, Guid actor, CancellationToken ct)
    {
        CompanyProfile x = await db.CompanyProfiles.SingleOrDefaultAsync(ct) ?? new CompanyProfile(); if (db.Entry(x).State == EntityState.Detached) db.CompanyProfiles.Add(x);
        x.CompanyName=r.CompanyName; x.LogoUrl=r.LogoUrl; x.Address=r.Address; x.GstNumber=r.GstNumber; x.Pan=r.Pan; x.Phone=r.Phone; x.Email=r.Email; x.Website=r.Website; x.DefaultCurrency=r.DefaultCurrency; x.DefaultTimezone=r.DefaultTimezone; x.SmtpHost=r.SmtpHost; x.SmtpPort=r.SmtpPort; x.SmtpUserName=r.SmtpUserName; if (!string.IsNullOrWhiteSpace(r.SmtpPassword)) x.SmtpPasswordEncrypted=_smtpProtector.Protect(r.SmtpPassword); x.SmtpUseTls=r.SmtpUseTls; x.BarcodePrefix=r.BarcodePrefix; x.PackingSlipFooter=r.PackingSlipFooter; x.ReportHeader=r.ReportHeader; x.ReportFooter=r.ReportFooter; x.ModifiedAtUtc=DateTimeOffset.UtcNow; x.ModifiedBy=actor;
        await Audit(actor, null, "Configuration", "Company Profile Updated", "Success", null, null, ct);
    }

    private async Task<UserIdentityDto> Identity(ApplicationUser user, CancellationToken ct) { string[] roleNames = (await users.GetRolesAsync(user)).ToArray(); string[] permissions = await db.RolePermissions.Where(x => roleNames.Contains(x.Role.Name!)).Select(x => x.Permission.Name).Distinct().ToArrayAsync(ct); return new(user.Id, user.Email!, user.DisplayName, user.MustChangePassword, roleNames, permissions); }
    private async Task<ApplicationUser> FindUser(Guid id) => await users.FindByIdAsync(id.ToString()) ?? throw new NotFoundException("User not found.");
    private async Task<ApplicationRole> FindRole(Guid id) => await roles.FindByIdAsync(id.ToString()) ?? throw new NotFoundException("Role not found.");
    private static void Ensure(IdentityResult result) { if (!result.Succeeded) throw new ValidationException(result.Errors.Select(x => x.Description).ToArray()); }
    private async Task Audit(Guid? userId, string? email, string category, string action, string outcome, string? ip, string? details, CancellationToken ct, bool save = true) { db.AuditLogs.Add(new AuditLog { UserId=userId, UserEmail=email, Category=category, Action=action, Outcome=outcome, IpAddress=ip, Details=details }); if (save) await db.SaveChangesAsync(ct); }
    private static string MaskEmail(string email) { string[] parts=email.Split('@'); return parts[0][..Math.Min(2,parts[0].Length)] + "***@" + parts[1]; }
    private static CompanyProfileDto Map(CompanyProfile x) => new(x.Id,x.CompanyName,x.LogoUrl,x.Address,x.GstNumber,x.Pan,x.Phone,x.Email,x.Website,x.DefaultCurrency,x.DefaultTimezone,x.SmtpHost,x.SmtpPort,x.SmtpUserName,x.SmtpUseTls,x.BarcodePrefix,x.PackingSlipFooter,x.ReportHeader,x.ReportFooter);
}

# Coil Manager Security Platform

## Runtime baseline

The repository remains on .NET 8 and Angular 22 because those are the installed and current project runtimes. The security design is compatible with a later .NET upgrade.

## Architecture

- `Application/Security` contains API-neutral DTOs, commands, validators, handlers, and service ports.
- `Persistence/Identity` contains ASP.NET Core Identity stores and the transactional security implementation.
- `Infrastructure/Auth` signs JWTs and performs cryptographic token/OTP hashing.
- `API/Authorization` resolves `Permission:<name>` policies dynamically from JWT permission claims.
- Angular `core/auth` owns signal state, guards, interception, refresh, and session cleanup.
- Angular `features/auth` and `features/admin/security` provide public authentication and protected administration surfaces.

Legacy placeholder `auth.Users`, `auth.Roles`, and `auth.UserRoles` tables are preserved. Identity uses dedicated `auth.Identity*` tables so migration does not reinterpret legacy columns.

## Token and authentication controls

- Access token lifetime: 15 minutes; JWT validation clock skew: 30 seconds.
- Refresh token lifetime: 30 days; only SHA-256 hashes are stored server-side and the browser receives the credential only in an HttpOnly, SameSite cookie.
- Every refresh rotates the token and revokes the predecessor.
- The active flow authenticates with email and password and issues a session immediately. Email OTP infrastructure remains dormant until SMTP is available.
- Dormant OTP records use cryptographically random six-digit values, HMAC-SHA256 hashing, five-minute expiry, five attempts, and latest-code-only validation.
- Logout-all and deactivation revoke every live session.

## Initial administrator

No known or default credential is committed. For an empty Identity store, provide these as deployment secrets before the first migration-enabled startup:

```powershell
$env:BootstrapAdmin__Email='administrator@example.com'
$env:BootstrapAdmin__Password='<strong unique deployment secret>'
```

The account is created in the Administrator role and must change its password. Remove the secrets immediately after provisioning.

## Required production secrets

Override `JwtSettings__SecretKey` and `Security__OtpHashKey` using independent high-entropy secrets. Do not deploy the placeholder configuration values. Persist Data Protection keys outside the application instance so encrypted SMTP credentials remain decryptable after restart and across replicas.

## Email

Password-reset mail and the dormant OTP flow use the Company Profile SMTP configuration. SMTP is not required for ordinary password login. The SMTP password is protected with ASP.NET Core Data Protection and is never returned by APIs.

## Permission model

Permissions belong only to roles. Users receive permissions by role membership; direct user grants are intentionally unsupported. Administrator is treated as a break-glass full-access system role. New modules should add stable `<Area>.<Resource>.<Action>` permission names and use `Authorize(Policy = "Permission:<name>")`.

## Migration and deployment

The additive `AddEnterpriseSecurityPlatform` migration creates Identity, permission, OTP, session, audit, and company-profile tables. Review and back up production data before applying any migration. Set `Database:ApplyMigrationsOnStartup` according to the deployment strategy; production environments commonly apply reviewed migrations in a separate release step.

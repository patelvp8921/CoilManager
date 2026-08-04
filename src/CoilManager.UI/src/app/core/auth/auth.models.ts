export interface ApiResponse<T> { success: boolean; message: string; data: T; errors: string[]; }
export interface LoginChallenge { challengeId: string; maskedEmail: string; expiresAtUtc: string; resendAfterSeconds: number; }
export interface UserIdentity { id: string; email: string; displayName: string; mustChangePassword: boolean; roles: string[]; permissions: string[]; }
export interface TokenResponse { accessToken: string; accessTokenExpiresAtUtc: string; refreshToken: string; refreshTokenExpiresAtUtc: string; user: UserIdentity; }
export interface UserSummary { id:string; email:string; displayName:string; jobTitle?:string; isActive:boolean; isLocked:boolean; lastLoginAtUtc?:string; createdAtUtc:string; roles:string[]; }
export interface Role { id:string; name:string; description?:string; isSystem:boolean; permissions:string[]; assignedUsers:number; }
export interface Session { id:string; userId:string; userEmail:string; device?:string; browser?:string; ipAddress?:string; loginAtUtc:string; lastActivityAtUtc:string; isCurrent:boolean; isRevoked:boolean; }
export interface AuditLog { id:string; timestampUtc:string; userEmail?:string; category:string; action:string; outcome:string; ipAddress?:string; details?:string; }

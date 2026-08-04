using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using CoilManager.Application.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace CoilManager.Infrastructure.Auth;

public sealed class SecurityTokenService(IConfiguration configuration) : ISecurityTokenService
{
    public (string Token, DateTimeOffset ExpiresAtUtc) CreateAccessToken(UserIdentityDto user, Guid sessionId)
    {
        IConfigurationSection settings = configuration.GetSection("JwtSettings");
        string keyValue = settings["SecretKey"] ?? settings["SigningKey"]
            ?? throw new InvalidOperationException("JWT signing key is required.");
        DateTimeOffset expires = DateTimeOffset.UtcNow.AddMinutes(settings.GetValue("AccessTokenMinutes", 15));
        List<Claim> claims =
        [
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()), new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), new("sid", sessionId.ToString()),
            new(ClaimTypes.Name, user.DisplayName), new("security_stamp", Guid.NewGuid().ToString())
        ];
        claims.AddRange(user.Roles.Select(x => new Claim(ClaimTypes.Role, x)));
        claims.AddRange(user.Permissions.Select(x => new Claim("permission", x)));
        SymmetricSecurityKey key = new(Encoding.UTF8.GetBytes(keyValue));
        JwtSecurityToken jwt = new(settings["Issuer"], settings["Audience"], claims, expires: expires.UtcDateTime,
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));
        return (new JwtSecurityTokenHandler().WriteToken(jwt), expires);
    }

    public string CreateRefreshToken() => Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
    public string HashSecret(string value) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));
    public string HashOtp(Guid challengeId, string code)
    {
        string secret = configuration["Security:OtpHashKey"] ?? configuration["JwtSettings:SecretKey"]
            ?? throw new InvalidOperationException("OTP hashing key is required.");
        using HMACSHA256 hmac = new(Encoding.UTF8.GetBytes(secret));
        return Convert.ToHexString(hmac.ComputeHash(Encoding.UTF8.GetBytes($"{challengeId:N}:{code}")));
    }
}

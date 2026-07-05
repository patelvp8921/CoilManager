using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CoilManager.Application.DTOs.Auth;
using CoilManager.Application.Interfaces.Auth;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace CoilManager.Infrastructure.Auth;

public sealed class JwtTokenService(IConfiguration configuration) : IJwtTokenService
{
    public LoginResponse GenerateToken(string userName, IReadOnlyCollection<string> roles)
    {
        IConfigurationSection jwtSection = configuration.GetSection("JwtSettings");
        string issuer = jwtSection["Issuer"] ?? "CoilManager";
        string audience = jwtSection["Audience"] ?? "CoilManager.Client";
        string secretKey = jwtSection["SecretKey"] ?? jwtSection["SigningKey"] ?? "CHANGE_ME_TO_A_SECURE_256_BIT_SECRET";
        int expiryMinutes = int.TryParse(jwtSection["ExpiryMinutes"], out int configuredExpiryMinutes)
            ? configuredExpiryMinutes
            : 60;

        DateTimeOffset expiresAtUtc = DateTimeOffset.UtcNow.AddMinutes(expiryMinutes);

        List<Claim> claims =
        [
            new(JwtRegisteredClaimNames.Sub, userName),
            new(ClaimTypes.Name, userName)
        ];
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        SymmetricSecurityKey key = new(Encoding.UTF8.GetBytes(secretKey));
        SigningCredentials credentials = new(key, SecurityAlgorithms.HmacSha256);

        JwtSecurityToken token = new(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expiresAtUtc.UtcDateTime,
            signingCredentials: credentials);

        return new LoginResponse(
            new JwtSecurityTokenHandler().WriteToken(token),
            expiresAtUtc,
            userName,
            roles);
    }
}

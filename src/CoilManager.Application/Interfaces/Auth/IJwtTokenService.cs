using CoilManager.Application.DTOs.Auth;

namespace CoilManager.Application.Interfaces.Auth;

public interface IJwtTokenService
{
    LoginResponse GenerateToken(string userName, IReadOnlyCollection<string> roles);
}

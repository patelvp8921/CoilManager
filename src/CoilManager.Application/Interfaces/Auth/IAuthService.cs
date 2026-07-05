using CoilManager.Application.DTOs.Auth;
using CoilManager.Shared.Results;

namespace CoilManager.Application.Interfaces.Auth;

public interface IAuthService
{
    Task<Result<LoginResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
}

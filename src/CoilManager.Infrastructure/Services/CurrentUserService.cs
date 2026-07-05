using System.Security.Claims;
using CoilManager.Application.Interfaces.Services;
using Microsoft.AspNetCore.Http;

namespace CoilManager.Infrastructure.Services;

public sealed class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    public string? UserId => User?.FindFirstValue(ClaimTypes.NameIdentifier);
    public string? UserName => User?.Identity?.Name;
    public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;

    private ClaimsPrincipal? User => httpContextAccessor.HttpContext?.User;
}

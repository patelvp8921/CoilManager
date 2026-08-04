using CoilManager.Application.Security;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoilManager.API.Controllers;

[Route("api/[controller]")]
public sealed class AuthController(IMediator mediator, ISecurityPlatformService security) : BaseApiController
{
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult> Login([FromBody] LoginCommand request, CancellationToken ct)
    {
        TokenResponseDto result = await security.LoginAsync(request.Email, request.Password, request.RememberMe, null, Request.Headers.UserAgent, HttpContext.Connection.RemoteIpAddress?.ToString(), ct);
        SetRefreshCookie(result, request.RememberMe);
        return SuccessResult(result with { RefreshToken = string.Empty });
    }

    [AllowAnonymous, HttpPost("otp/verify")]
    public async Task<ActionResult> Verify([FromBody] VerifyOtpCommand request, CancellationToken ct) { TokenResponseDto result=await security.VerifyOtpAsync(request.ChallengeId, request.Code, request.DeviceName, Request.Headers.UserAgent, HttpContext.Connection.RemoteIpAddress?.ToString(), ct); SetRefreshCookie(result); return SuccessResult(result with { RefreshToken = string.Empty }); }
    [AllowAnonymous, HttpPost("otp/resend")]
    public async Task<ActionResult> Resend([FromBody] ResendOtpRequest request, CancellationToken ct) => SuccessResult(await security.ResendOtpAsync(request.ChallengeId, HttpContext.Connection.RemoteIpAddress?.ToString(), ct));
    [AllowAnonymous, HttpPost("refresh")]
    public async Task<ActionResult> Refresh([FromBody] RefreshRequest? request, CancellationToken ct) { string value=Request.Cookies["cm.refresh"]??request?.RefreshToken??string.Empty; TokenResponseDto result=await security.RefreshAsync(value,HttpContext.Connection.RemoteIpAddress?.ToString(),ct); SetRefreshCookie(result); return SuccessResult(result with { RefreshToken = string.Empty }); }
    [Authorize, HttpPost("logout")]
    public async Task<ActionResult> Logout(CancellationToken ct) { await security.LogoutAsync(ClaimGuid("sid"), ClaimGuid(System.Security.Claims.ClaimTypes.NameIdentifier, "sub"), ct); Response.Cookies.Delete("cm.refresh"); return NoContent(); }
    [Authorize, HttpPost("logout-all")]
    public async Task<ActionResult> LogoutAll(CancellationToken ct) { await security.LogoutAllAsync(ClaimGuid(System.Security.Claims.ClaimTypes.NameIdentifier, "sub"), ct); return NoContent(); }
    [AllowAnonymous, HttpPost("forgot-password")]
    public async Task<ActionResult> Forgot([FromBody] ForgotPasswordCommand request, CancellationToken ct) { await mediator.Send(request, ct); return Accepted(); }
    [AllowAnonymous, HttpPost("reset-password")]
    public async Task<ActionResult> Reset([FromBody] ResetPasswordCommand request, CancellationToken ct) { await mediator.Send(request, ct); return NoContent(); }
    private Guid ClaimGuid(params string[] types) { string? value = types.Select(t => User.FindFirst(t)?.Value).FirstOrDefault(x => x is not null); return Guid.TryParse(value, out Guid id) ? id : throw new UnauthorizedAccessException(); }
    private void SetRefreshCookie(TokenResponseDto result, bool persistent=true) => Response.Cookies.Append("cm.refresh", result.RefreshToken, new CookieOptions { HttpOnly=true, Secure=Request.IsHttps, SameSite=SameSiteMode.Strict, Expires=persistent?result.RefreshTokenExpiresAtUtc:null, Path="/api/auth" });
}
public sealed record ResendOtpRequest(Guid ChallengeId);
public sealed record RefreshRequest(string? RefreshToken);

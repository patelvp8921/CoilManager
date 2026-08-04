using FluentValidation;
using MediatR;

namespace CoilManager.Application.Security;

public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator() { RuleFor(x => x.Email).NotEmpty().EmailAddress(); RuleFor(x => x.Password).NotEmpty(); }
}
public sealed class VerifyOtpCommandValidator : AbstractValidator<VerifyOtpCommand>
{
    public VerifyOtpCommandValidator() { RuleFor(x => x.ChallengeId).NotEmpty(); RuleFor(x => x.Code).Matches("^[0-9]{6}$"); }
}
public sealed class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordCommandValidator() { RuleFor(x => x.Email).EmailAddress(); RuleFor(x => x.Token).NotEmpty(); RuleFor(x => x.NewPassword).MinimumLength(12); }
}

public sealed class LoginCommandHandler(ISecurityPlatformService security) : IRequestHandler<LoginCommand, TokenResponseDto>
{
    public Task<TokenResponseDto> Handle(LoginCommand request, CancellationToken ct) => security.LoginAsync(request.Email, request.Password, request.RememberMe, null, null, null, ct);
}
public sealed class VerifyOtpCommandHandler(ISecurityPlatformService security) : IRequestHandler<VerifyOtpCommand, TokenResponseDto>
{
    public Task<TokenResponseDto> Handle(VerifyOtpCommand request, CancellationToken ct) => security.VerifyOtpAsync(request.ChallengeId, request.Code, request.DeviceName, null, null, ct);
}
public sealed class RefreshTokenCommandHandler(ISecurityPlatformService security) : IRequestHandler<RefreshTokenCommand, TokenResponseDto>
{
    public Task<TokenResponseDto> Handle(RefreshTokenCommand request, CancellationToken ct) => security.RefreshAsync(request.RefreshToken, null, ct);
}
public sealed class ForgotPasswordCommandHandler(ISecurityPlatformService security) : IRequestHandler<ForgotPasswordCommand>
{
    public async Task Handle(ForgotPasswordCommand request, CancellationToken ct) => await security.ForgotPasswordAsync(request.Email, ct);
}
public sealed class ResetPasswordCommandHandler(ISecurityPlatformService security) : IRequestHandler<ResetPasswordCommand>
{
    public async Task Handle(ResetPasswordCommand request, CancellationToken ct) => await security.ResetPasswordAsync(request.Email, request.Token, request.NewPassword, ct);
}

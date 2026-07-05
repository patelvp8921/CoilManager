namespace CoilManager.Application.DTOs.Auth;

public sealed record LoginResponse(
    string AccessToken,
    DateTimeOffset ExpiresAtUtc,
    string UserName,
    IReadOnlyCollection<string> Roles);

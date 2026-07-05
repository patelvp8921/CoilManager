namespace CoilManager.API.Configuration;

public sealed class JwtSettings
{
    public const string SectionName = "JwtSettings";

    public string Issuer { get; init; } = string.Empty;
    public string Audience { get; init; } = string.Empty;
    public string SecretKey { get; init; } = string.Empty;
    public string SigningKey { get; init; } = string.Empty;
    public string EffectiveSecretKey => !string.IsNullOrWhiteSpace(SecretKey) ? SecretKey : SigningKey;
    public int ExpiryMinutes { get; init; } = 60;
}

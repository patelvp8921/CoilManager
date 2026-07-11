namespace CoilManager.Application.Settings;

public sealed class SlittingSettings
{
    public const string SectionName = "SlittingSettings";

    public decimal WeightToleranceKg { get; init; } = 0.5m;
    public decimal WidthToleranceMm { get; init; } = 0.5m;
    public decimal MinimumBalanceWidthMm { get; init; } = 10m;
    public string DefaultLabelVersion { get; init; } = "1";
}

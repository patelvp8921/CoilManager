namespace CoilManager.Application.Settings;

public sealed class LabelSettings
{
    public const string SectionName = "LabelSettings";
    public decimal WidthMm { get; init; } = 100;
    public decimal HeightMm { get; init; } = 75;
    public int DefaultCopies { get; init; } = 1;
    public string DefaultLabelVersion { get; init; } = "1";
    public string CompanyName { get; init; } = "Arkon Electricals Private Limited";
    public string? CompanyAddress { get; init; }
    public string? CompanyLogoUrl { get; init; }
    public bool ShowQrCode { get; init; } = true;
    public bool ShowBarcode { get; init; } = true;
    public string BarcodeFormat { get; init; } = "Code128";
    public string QrCodeErrorCorrectionLevel { get; init; } = "M";
    public string PrintOrientation { get; init; } = "Landscape";
    public int? PrinterDpi { get; init; }
}

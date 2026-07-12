using CoilManager.Domain.Enums;

namespace CoilManager.Application.DTOs.SlitCoils;

public sealed record SlitCoilLabelDto(Guid SlitCoilId, string CoilNumber, string MotherCoilNumber,
    string SlittingJobNo, string? Grade, decimal Thickness, string Category, decimal CoreLossPerKg,
    decimal Width, decimal Weight, string? Supplier, string? Manufacturer, string HeatNumber,
    string BarcodeValue, string QrCodeValue, string LabelVersion, bool LabelPrinted,
    int LabelPrintCount, DateTimeOffset? LabelLastPrintedOn, string? LabelLastPrintedBy,
    string CompanyName, string? CompanyAddress, string? CompanyLogoUrl, decimal LabelWidthMm,
    decimal LabelHeightMm);

public sealed record PrintSlitCoilLabelRequest(int Copies = 1, string? PrinterName = null, string? Remarks = null);
public sealed record PrintSlitCoilLabelResultDto(Guid SlitCoilId, string CoilNumber, string LabelVersion,
    int PrintCount, DateTimeOffset PrintedOn, string? PrintedBy, int Copies, LabelPrintType PrintType);
public sealed record BatchPrintSlitCoilLabelsRequest(IReadOnlyList<Guid> SlitCoilIds, int CopiesPerLabel = 1,
    string? PrinterName = null, string? Remarks = null);
public sealed record BatchPrintFailureDto(Guid SlitCoilId, string Reason);
public sealed record BatchPrintSlitCoilLabelsResultDto(int TotalRequested, int TotalPrinted,
    IReadOnlyList<BatchPrintFailureDto> Failed, IReadOnlyList<PrintSlitCoilLabelResultDto> Labels);
public sealed record IncrementLabelVersionRequest(string Reason);
public sealed record LabelPrintHistoryDto(DateTimeOffset PrintedOn, string? PrintedBy, int Copies,
    string LabelVersion, string? PrinterName, LabelPrintType PrintType, string? Remarks);

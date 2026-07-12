using CoilManager.Domain.Enums;

namespace CoilManager.Application.DTOs.SlitCoils;

public sealed record SlitCoilQueryRequest(
    int Page = 1, int PageSize = 25, string? Search = null, string? CoilNumber = null,
    string? MotherCoilNumber = null, string? SlittingJobNo = null, Guid? GradeId = null,
    CoilStatus? Status = null, Guid? SupplierId = null, Guid? ManufacturerId = null,
    decimal? Thickness = null, decimal? WidthFrom = null, decimal? WidthTo = null,
    decimal? WeightFrom = null, decimal? WeightTo = null, DateTimeOffset? CreatedFrom = null,
    DateTimeOffset? CreatedTo = null, string? SortBy = null, string? SortDirection = null)
{
    public int NormalizedPage => Math.Max(1, Page);
    public int NormalizedPageSize => Math.Clamp(PageSize, 1, 100);
    public bool SortDescending => string.Equals(SortDirection, "desc", StringComparison.OrdinalIgnoreCase);
}

public sealed record SlitCoilDto(
    Guid Id, string CoilNumber, Guid MotherCoilId, string MotherCoilNumber,
    Guid SlittingJobId, string SlittingJobNo, string? Grade, decimal Thickness,
    string Category, decimal CoreLossPerKg, decimal Width, decimal Weight,
    CoilStatus Status, string? WarehouseLocation, string BarcodeValue, string QrCodeValue,
    string LabelVersion, DateTimeOffset CreatedOn);

public sealed record SlitCoilListItemDto(
    Guid Id, string CoilNumber, string MotherCoilNumber, Guid MotherCoilId,
    string SlittingJobNo, Guid SlittingJobId, string? Grade, decimal Thickness,
    string Category, decimal Width, decimal Weight, string? Supplier, string? Manufacturer,
    CoilStatus Status, string? WarehouseLocation, DateTimeOffset CreatedOn,
    string LabelVersion, bool LabelPrinted, int LabelPrintCount, DateTimeOffset? LabelLastPrintedOn,
    bool HasBarcode, bool HasQrCode);

public sealed record SlitCoilDetailsDto(
    Guid Id, string CoilNumber, CoilStatus Status, int GenerationLevel, int SlitSequence,
    string LabelVersion, string BarcodeValue, string QrCodeValue, DateTimeOffset CreatedOn,
    string? CreatedBy, DateTimeOffset? ModifiedOn, string? ModifiedBy, string? Grade,
    decimal Thickness, string Category, decimal CoreLossPerKg, decimal Width, decimal Weight,
    string HeatNumber, string? Supplier, string? Manufacturer, string? WarehouseLocation,
    Guid ParentCoilId, string ParentCoilNumber, Guid RootMotherCoilId,
    string RootMotherCoilNumber, Guid MotherCoilId, string MotherCoilNumber,
    Guid SlittingJobId, string SlittingJobNo, bool CanViewMotherCoil,
    bool CanViewParentCoil, bool CanViewSlittingJob, bool CanPrintLabel);

public sealed record SlitCoilGenealogyDto(
    Guid Id, string CoilNumber, string ParentCoilNumber, string RootMotherCoilNumber,
    string MotherCoilNumber, string SlittingJobNo, int SlitSequence, int GenerationLevel);

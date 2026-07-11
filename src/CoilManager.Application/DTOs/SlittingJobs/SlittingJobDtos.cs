using CoilManager.Domain.Enums;

namespace CoilManager.Application.DTOs.SlittingJobs;

public sealed record SlittingJobQueryRequest(
    int Page = 1,
    int PageSize = 25,
    string? Search = null,
    SlittingJobStatus? Status = null,
    string? SortBy = null,
    string? SortDirection = null)
{
    public int NormalizedPage => Page < 1 ? 1 : Page;
    public int NormalizedPageSize => PageSize switch
    {
        < 1 => 25,
        > 100 => 100,
        _ => PageSize
    };

    public bool SortDescending => string.Equals(SortDirection, "desc", StringComparison.OrdinalIgnoreCase);
}

public sealed record CreateSlittingJobRequest(
    DateOnly PlanningDate,
    string? PlannerId,
    Guid MotherCoilId,
    Guid? MachineId,
    string? Shift,
    decimal KnifeThickness,
    decimal LeftEdgeTrim,
    decimal RightEdgeTrim,
    string? Remarks,
    IReadOnlyList<SlittingJobItemRequest> Items);

public sealed record UpdateSlittingJobRequest(
    DateOnly PlanningDate,
    string? PlannerId,
    Guid MotherCoilId,
    Guid? MachineId,
    string? Shift,
    decimal KnifeThickness,
    decimal LeftEdgeTrim,
    decimal RightEdgeTrim,
    string? Remarks,
    IReadOnlyList<SlittingJobItemRequest> Items,
    string RowVersion);

public sealed record SlittingJobItemRequest(
    int SequenceNo,
    decimal Width,
    string? Remarks);

public sealed record SlittingJobDto(
    Guid Id,
    string SlittingJobNo,
    DateOnly PlanningDate,
    string? PlannerId,
    Guid MotherCoilId,
    string MotherCoilNo,
    string? SupplierCoilNumber,
    string? HeatNumber,
    string? SupplierName,
    string? ManufacturerName,
    string? Grade,
    decimal Thickness,
    string Category,
    decimal CoreLossPerKg,
    decimal MotherCoilWidth,
    decimal MotherCoilWeight,
    decimal MotherCoilLength,
    string? WarehouseLocation,
    CoilStatus MotherCoilStatus,
    Guid? MachineId,
    string? Shift,
    SlittingJobStatus Status,
    decimal KnifeThickness,
    decimal LeftEdgeTrim,
    decimal RightEdgeTrim,
    string? Remarks,
    decimal TotalPlannedWidth,
    decimal KnifeLoss,
    decimal EdgeTrim,
    decimal RemainingWidth,
    decimal UtilizationPercent,
    DateTimeOffset CreatedOn,
    string? CreatedBy,
    DateTimeOffset? ModifiedOn,
    string? ModifiedBy,
    string? ReleasedBy,
    DateTimeOffset? ReleasedOn,
    string? StartedBy,
    DateTimeOffset? StartedOn,
    string? CompletedBy,
    DateTimeOffset? CompletedOn,
    string? CancelledBy,
    DateTimeOffset? CancelledOn,
    string RowVersion,
    IReadOnlyList<SlittingJobItemDto> Items);

public sealed record SlittingJobItemDto(
    Guid Id,
    int SequenceNo,
    string SlitCoilId,
    decimal Width,
    decimal EstimatedWeight,
    SlittingJobStatus Status,
    string? Remarks);

public sealed record SlittingMotherCoilLookupDto(
    Guid Id,
    string MotherCoilId,
    string CoilNumber,
    string HeatNumber,
    string? SupplierName,
    string? ManufacturerName,
    string? Grade,
    decimal Thickness,
    string Category,
    decimal CoreLossPerKg,
    decimal Width,
    decimal Weight,
    decimal Length,
    string? WarehouseLocation,
    CoilStatus Status);

public sealed record CompleteSlittingRequest(
    string RowVersion,
    IReadOnlyList<CompleteSlittingItemRequest> Slits);

public sealed record StartSlittingRequest(
    string RowVersion,
    Guid? MachineId,
    string? Shift,
    string? Remarks);

public sealed record StartSlittingResponse(
    Guid SlittingJobId,
    string SlittingJobNo,
    SlittingJobStatus Status,
    string MotherCoilNumber,
    CoilStatus MotherCoilStatus,
    string? StartedBy,
    DateTimeOffset StartedOn);

public sealed record CompleteSlittingItemRequest(
    Guid SlittingJobItemId,
    decimal ActualWeight,
    decimal? ActualWidth,
    string? Remarks);

public sealed record CompleteSlittingResponse(
    Guid SlittingJobId,
    string SlittingJobNo,
    string MotherCoilNumber,
    IReadOnlyList<GeneratedSlitCoilDto> GeneratedSlitCoils,
    decimal TotalGeneratedWeight,
    DateTimeOffset CompletedOn,
    decimal RemainingWidth,
    decimal UnusedEstimatedWeight,
    IReadOnlyList<string> Warnings);

public sealed record GeneratedSlitCoilDto(
    Guid Id,
    string CoilNumber,
    string ParentCoilNumber,
    string MotherCoilNumber,
    string SlittingJobNo,
    decimal Width,
    decimal Weight,
    string? Grade,
    decimal Thickness,
    string Category,
    decimal CoreLossPerKg,
    CoilStatus Status,
    string BarcodeValue,
    string QrCodeValue,
    string LabelVersion);

public sealed record SlittingJobCompletionDto(
    Guid SlittingJobId,
    string SlittingJobNo,
    SlittingJobStatus Status,
    string MotherCoilNumber,
    decimal TotalGeneratedWeight,
    IReadOnlyList<GeneratedSlitCoilDto> GeneratedSlitCoils);

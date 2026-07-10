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

using CoilManager.Domain.Enums;

namespace CoilManager.Application.DTOs.SlitCoils;

public sealed record SlitCoilQueryRequest(
    int Page = 1,
    int PageSize = 25,
    string? Search = null,
    CoilStatus? Status = null,
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

public sealed record SlitCoilDto(
    Guid Id,
    string CoilNumber,
    Guid MotherCoilId,
    string MotherCoilNumber,
    Guid SlittingJobId,
    string SlittingJobNo,
    string? Grade,
    decimal Thickness,
    string Category,
    decimal CoreLossPerKg,
    decimal Width,
    decimal Weight,
    CoilStatus Status,
    string? WarehouseLocation,
    string BarcodeValue,
    string QrCodeValue,
    string LabelVersion,
    DateTimeOffset CreatedOn);

public sealed record SlitCoilGenealogyDto(
    Guid Id,
    string CoilNumber,
    string ParentCoilNumber,
    string RootMotherCoilNumber,
    string MotherCoilNumber,
    string SlittingJobNo,
    int SlitSequence,
    int GenerationLevel);

using CoilManager.Domain.Enums;

namespace CoilManager.Application.DTOs.RawCoils;

public sealed record RawCoilDto(
    Guid Id,
    string CoilID,
    string CoilNumber,
    string HeatNumber,
    string MillName,
    string? MillTCNo,
    string? BISLicNumber,
    string SupplierName,
    string Grade,
    decimal Thickness,
    decimal Width,
    decimal Weight,
    decimal Length,
    decimal WattLossPerKg,
    string? WarehouseLocation,
    CoilStatus Status,
    DateOnly ReceivedDate,
    DateTimeOffset CreatedOn,
    string? CreatedBy,
    DateTimeOffset? ModifiedOn,
    string? ModifiedBy,
    bool IsDeleted,
    DateTimeOffset? DeletedOn,
    string RowVersion,
    IReadOnlyList<string> DocumentPlaceholders);

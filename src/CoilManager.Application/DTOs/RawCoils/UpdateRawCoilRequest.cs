using CoilManager.Domain.Enums;

namespace CoilManager.Application.DTOs.RawCoils;

public sealed record UpdateRawCoilRequest(
    string CoilNumber,
    string HeatNumber,
    string? PONumber,
    string? InvoiceNo,
    string? MillTCNo,
    string? BISLicNumber,
    Guid SupplierId,
    Guid ManufacturerId,
    Guid GradeId,
    decimal? Thickness,
    decimal? Width,
    decimal Weight,
    decimal Length,
    decimal? WattLossPerKg,
    string? WarehouseLocation,
    CoilStatus Status,
    DateOnly ReceivedDate,
    string RowVersion);

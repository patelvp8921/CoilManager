namespace CoilManager.Application.DTOs.RawCoils;

public sealed record CreateRawCoilRequest(
    string CoilNumber,
    string HeatNumber,
    string MillName,
    string? MillTCNo,
    string? BISLicNumber,
    string SupplierName,
    string Grade,
    decimal? Thickness,
    decimal? Width,
    decimal Weight,
    decimal Length,
    decimal? WattLossPerKg,
    string? WarehouseLocation,
    DateOnly ReceivedDate);

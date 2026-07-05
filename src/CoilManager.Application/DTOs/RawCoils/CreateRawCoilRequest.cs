namespace CoilManager.Application.DTOs.RawCoils;

public sealed record CreateRawCoilRequest(
    string CoilNumber,
    string HeatNumber,
    string SupplierName,
    string Grade,
    decimal ThicknessMm,
    decimal WidthMm,
    decimal WeightMt,
    DateOnly ReceivedDate);

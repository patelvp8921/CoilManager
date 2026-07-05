using CoilManager.Domain.Enums;

namespace CoilManager.Application.DTOs.RawCoils;

public sealed record RawCoilDto(
    Guid Id,
    string CoilNumber,
    string HeatNumber,
    string SupplierName,
    string Grade,
    decimal ThicknessMm,
    decimal WidthMm,
    decimal WeightMt,
    CoilStatus Status,
    string? Warehouse,
    string? Location,
    DateOnly ReceivedDate);

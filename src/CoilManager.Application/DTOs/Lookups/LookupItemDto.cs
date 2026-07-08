namespace CoilManager.Application.DTOs.Lookups;

public sealed record LookupItemDto(
    Guid Id,
    string Code,
    string Name,
    decimal? ThicknessMm = null,
    string? Category = null,
    decimal? CoreLossPerKg = null);

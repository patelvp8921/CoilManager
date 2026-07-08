namespace CoilManager.Application.DTOs.Masters;

public sealed record MasterDto(
    Guid Id,
    string Code,
    string Name,
    string? Description,
    string? Country,
    string? Address,
    string? GST,
    string? Email,
    string? ContactNo,
    bool IsActive,
    DateTimeOffset CreatedOn,
    string? CreatedBy,
    DateTimeOffset? ModifiedOn,
    string? ModifiedBy,
    string RowVersion,
    string? Grade = null,
    decimal? ThicknessMm = null,
    string? Category = null,
    decimal? CoreLossPerKg = null);

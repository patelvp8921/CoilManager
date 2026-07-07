namespace CoilManager.Application.DTOs.Masters;

public sealed record UpdateMasterRequest(
    string? Code,
    string Name,
    string? Description,
    string? Country,
    string? Address,
    string? GST,
    string? Email,
    string? ContactNo,
    bool IsActive,
    string? RowVersion);

namespace CoilManager.Application.DTOs.Masters;

public sealed record CreateMasterRequest(
    string? Code,
    string Name,
    string? Description,
    string? Country,
    string? Address,
    string? GST,
    string? Email,
    string? ContactNo,
    bool IsActive = true);

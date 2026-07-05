using CoilManager.Shared.Pagination;

namespace CoilManager.Shared.Responses;

public sealed record ApiPagedResponse<T>(
    bool Success,
    string Message,
    IReadOnlyList<T> Data,
    PaginationResult Pagination,
    IReadOnlyList<string> Errors);

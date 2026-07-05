namespace CoilManager.Shared.Pagination;

public sealed record PaginationRequest(
    int Page = 1,
    int PageSize = 10,
    string? SortBy = null,
    string? SortDirection = null,
    string? Search = null)
{
    public int NormalizedPage => Page < 1 ? 1 : Page;
    public int NormalizedPageSize => PageSize is < 1 or > 200 ? 10 : PageSize;
    public bool IsDescending => string.Equals(SortDirection, "desc", StringComparison.OrdinalIgnoreCase);
}

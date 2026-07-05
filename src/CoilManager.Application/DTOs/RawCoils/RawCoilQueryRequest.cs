using CoilManager.Domain.Enums;

namespace CoilManager.Application.DTOs.RawCoils;

public sealed record RawCoilQueryRequest
{
    private const int DefaultPage = 1;
    private const int DefaultPageSize = 25;
    private const int MaxPageSize = 100;

    public int Page { get; init; } = DefaultPage;
    public int PageSize { get; init; } = DefaultPageSize;
    public string? Search { get; init; }
    public string? Grade { get; init; }
    public string? Manufacturer { get; init; }
    public CoilStatus? Status { get; init; }
    public string? SortBy { get; init; }
    public string? SortDirection { get; init; }

    public int NormalizedPage => Page < 1 ? DefaultPage : Page;
    public int NormalizedPageSize => PageSize switch
    {
        < 1 => DefaultPageSize,
        > MaxPageSize => MaxPageSize,
        _ => PageSize
    };

    public bool SortDescending => string.Equals(SortDirection, "desc", StringComparison.OrdinalIgnoreCase);
}

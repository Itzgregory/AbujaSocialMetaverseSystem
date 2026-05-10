namespace AbujaSocialMetaverse.Shared.Models;

/// <summary>
/// Wraps any list response with pagination metadata.
/// No endpoint returns an unbounded list.
/// </summary>
public class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; }
    public int TotalCount { get; }
    public int Page { get; }
    public int PageSize { get; }
    public int TotalPages { get; }
    public bool HasNextPage { get; }
    public bool HasPreviousPage { get; }

    private PagedResult(
        IReadOnlyList<T> items,
        int totalCount,
        int page,
        int pageSize)
    {
        Items = items;
        TotalCount = totalCount;
        Page = page;
        PageSize = pageSize;
        TotalPages = pageSize == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize);
        HasNextPage = page < TotalPages;
        HasPreviousPage = page > 1;
    }

    public static PagedResult<T> Create(
        IReadOnlyList<T> items,
        int totalCount,
        int page,
        int pageSize)
    {
        if (page < 1)
            throw new ArgumentOutOfRangeException(nameof(page),
                "Page number must be at least 1.");

        if (pageSize < 1)
            throw new ArgumentOutOfRangeException(nameof(pageSize),
                "Page size must be at least 1.");

        return new PagedResult<T>(items, totalCount, page, pageSize);
    }

    public static PagedResult<T> Empty(int page, int pageSize)
        => new([], 0, page, pageSize);

    /// <summary>
    /// Projects each item to a new type.
    /// </summary>
    public PagedResult<TOut> Map<TOut>(Func<T, TOut> mapper)
        => new(Items.Select(mapper).ToList().AsReadOnly(),
            TotalCount, Page, PageSize);
}

/// <summary>
/// Pagination request parameters.
/// Used on all list endpoints.
/// </summary>
public record PaginationRequest
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;

    public int Skip => (Page - 1) * PageSize;

    public void Validate()
    {
        if (Page < 1)
            throw new ArgumentOutOfRangeException(nameof(Page),
                "Page must be at least 1.");

        if (PageSize < 1 || PageSize > 100)
            throw new ArgumentOutOfRangeException(nameof(PageSize),
                "PageSize must be between 1 and 100.");
    }
}
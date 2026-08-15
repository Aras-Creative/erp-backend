namespace ArasERP.BuildingBlocks.Application;

public static class PaginationDefaults
{
    public const int Page = 1;

    public const int PageSize = 10;

    public const int MaxPageSize = 100;
}

public sealed class PagedList<T>
{
    public PagedList(IReadOnlyList<T> items, int page, int pageSize, int totalCount)
    {
        Items = items;
        Page = page;
        PageSize = pageSize;
        TotalCount = totalCount;
        TotalPages = totalCount > 0 ? (int)Math.Ceiling(totalCount / (double)pageSize) : 0;
    }

    public IReadOnlyList<T> Items { get; }

    public int Page { get; }

    public int PageSize { get; }

    public int TotalCount { get; }

    public int TotalPages { get; }

    public bool HasNextPage => Page < TotalPages;

    public bool HasPreviousPage => Page > 1;

    public PagedList<TResult> Map<TResult>(Func<T, TResult> selector) =>
        new(Items.Select(selector).ToList(), Page, PageSize, TotalCount);
}

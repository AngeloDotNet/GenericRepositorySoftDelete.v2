namespace Repository.Api.Repositories;

public class PagedResult<T>(IEnumerable<T> items, int totalCount, int page, int pageSize)
{
    public IEnumerable<T> Items { get; init; } = items;
    public int TotalCount { get; init; } = totalCount;
    public int Page { get; init; } = page;
    public int PageSize { get; init; } = pageSize;
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}
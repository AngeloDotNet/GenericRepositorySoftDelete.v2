namespace Repository.Api.Repositories;

public class QueryParameters
{
    private const int MaxPageSize = 100;
    private int pageSize = 10;

    public int Page { get; set; } = 1;
    public int PageSize
    {
        get => pageSize;
        set => pageSize = (value > MaxPageSize) ? MaxPageSize : value;
    }

    public string? SortBy { get; set; }
    public bool Desc { get; set; } = false;
}
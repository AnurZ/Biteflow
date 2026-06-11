namespace Market.Application.Common;

public sealed class PageRequest
{
    private const int MaxPageSize = 100;

    private int _page = 1;
    public int Page
    {
        get => _page;
        init => _page = value <= 0 ? 1 : value;
    }

    private int _pageSize = 10;
    public int PageSize
    {
        get => _pageSize;
        init => _pageSize = value <= 0 ? 10 : value > MaxPageSize ? MaxPageSize : value;
    }
    [JsonIgnore]
    public int SkipCount => (Page - 1) * PageSize;
}

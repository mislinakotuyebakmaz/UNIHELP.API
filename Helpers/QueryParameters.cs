// Helpers/QueryParameters.cs
namespace UniHelp.Api.Helpers;
public class QueryParameters
{
    private const int MaxPageSize = 50;
    public int PageNumber { get; set; } = 1;
    private int _pageSize = 10;
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = (value > MaxPageSize) ? MaxPageSize : value;
    }
    public string? SortBy { get; set; }
    public string? SearchTerm { get; set; } // Filtreleme için
}
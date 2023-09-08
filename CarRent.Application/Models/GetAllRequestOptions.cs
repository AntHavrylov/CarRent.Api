namespace CarRent.Application.Models;

public class GetAllRequestOptions
{
    public int Page { get; init; }

    public int PageSize { get; init; }

    public string? SortField { get; set; }

    public SortOrder? SortOrder { get; set; }
}

public enum SortOrder
{
    Unsorted,
    Ascending,
    Descending
}

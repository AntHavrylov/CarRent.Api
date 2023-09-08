namespace CarRent.Application.Models;

public class GetAllCarsOptions : GetAllRequestOptions
{
    public string? Slug { get; init; }

    public int? YearOfProduction { get; init; }
}


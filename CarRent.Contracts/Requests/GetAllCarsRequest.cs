namespace CarRent.Contracts.Requests;

public class GetAllCarsRequest : GetAllRequest
{
    public string? Slug { get; init; }

    public int? YearOfProduction { get; set; }
}

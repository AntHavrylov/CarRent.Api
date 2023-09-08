namespace CarRent.Contracts.Responses;

public class CarResponse
{
    public required Guid Id { get; init; }
    public required int YearOfProduction { get; init; }

    public required string Brand { get; init; }

    public required string Model { get; init; }

    public string Slug { get; init; }

    public required string EngineType { get; init; }
    public required string BodyType { get; init; }
}

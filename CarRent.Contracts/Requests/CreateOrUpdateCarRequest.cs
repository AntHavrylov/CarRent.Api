namespace CarRent.Contracts.Requests;

public class CreateOrUpdateCarRequest
{
    public required int YearOfProduction { get; init; }

    public required string Brand { get; init; }
    
    public required string Model { get; init; }
    
    public required string EngineType { get; init; }
    
    public required string BodyType { get; init; }

}

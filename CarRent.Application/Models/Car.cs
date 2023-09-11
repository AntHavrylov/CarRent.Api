using System.Text.RegularExpressions;

namespace CarRent.Application.Models;

public partial class Car
{
    public required Guid Id { get; init; }
    
    public required int YearOfProduction { get; init; }

    public float? rating { get; init; } = 0;
    
    public required string Brand { get; init; }

    public required string Model { get; init; }

    public string Slug => GenerateSlug();

    public required EngineType EngineType { get; init; }

    public required BodyType BodyType { get; init; }

    private string GenerateSlug()
    {
        var sluggedBrand = SlugRegex().Replace(Brand, string.Empty)
            .ToLower().Replace(" ", "-");
        var sluggedModel = SlugRegex().Replace(Model, string.Empty)
            .ToLower().Replace(" ", "-");
        return $"{sluggedBrand}-{sluggedModel}-{YearOfProduction}";
    }

    [GeneratedRegex("[^0-9A-Za-z _-]", RegexOptions.NonBacktracking, 5)]
    private static partial Regex SlugRegex();
}

public enum EngineType 
{
    Gasoline,
    Diesel,
    Hybrid,
    Electric
}

public enum BodyType 
{
    Sedan,
    HatchBack,
    Universal,
    Coupe,
    Suv,
    Crossover,
    Van,
    Micro
}


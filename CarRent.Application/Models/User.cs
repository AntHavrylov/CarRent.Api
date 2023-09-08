namespace CarRent.Application.Models;

public class User
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public required string Email { get; init; }

}

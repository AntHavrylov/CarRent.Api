namespace CarRent.Application.Models;

public class Order
{
    public required Guid Id { get; init; }

    public required Guid UserId { get; init; }

    public required Guid CarId { get; init; }

    public required DateTime DateFrom { get; init; }
    
    public required DateTime DateTo { get; init; }
}

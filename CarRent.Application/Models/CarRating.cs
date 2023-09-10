namespace CarRent.Application.Models;

public class CarRating
{
    public required Guid UserId { get; init; }

    public required Guid CarId { get; init; }    

    public required int Rating { get; init; }
}

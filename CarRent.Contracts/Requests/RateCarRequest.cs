namespace CarRent.Contracts.Requests;

public class RateCarRequest
{
    public required Guid CarId { get; init; }

    public required int Rating { get; init; }
}

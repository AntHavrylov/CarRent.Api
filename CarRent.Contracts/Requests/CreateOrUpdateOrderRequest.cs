namespace CarRent.Contracts.Requests;

public class CreateOrUpdateOrderRequest
{
    public required Guid CarId { get; init; }

    public required DateTime DateFrom { get; init; }

    public required DateTime DateTo { get; init; }
}

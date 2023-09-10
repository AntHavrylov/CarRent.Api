namespace CarRent.Contracts.Responses;

public class CarRatingsResponse
{
    public IEnumerable<CarRatingResponse> Items { get; set; } = new List<CarRatingResponse>();
}

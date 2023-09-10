namespace CarRent.Contracts.Responses;

public class OrdersResponse
{
    public IEnumerable<OrderResponse> Items { get; set; } = new List<OrderResponse>();
}

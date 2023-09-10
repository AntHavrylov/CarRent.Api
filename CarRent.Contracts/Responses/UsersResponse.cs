namespace CarRent.Contracts.Responses;

public class UsersResponse
{
    public IEnumerable<UserResponse>  Items { get; init; } = new List<UserResponse>();
}

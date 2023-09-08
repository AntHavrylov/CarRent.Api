namespace CarRent.Contracts.Requests;

public class CreateOrUpdateUserRequest
{
    public required string Name { get; init; }
    public required string Email { get; init; }
}

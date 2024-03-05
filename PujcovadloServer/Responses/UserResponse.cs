namespace PujcovadloServer.Responses;

public class UserResponse
{
    public string Id { get; set; } = default!;

    public string Username { get; set; } = default!;

    public string FirstName { get; set; } = default!;

    public string LastName { get; set; } = default!;

    public string Name => $"{FirstName} {LastName}";

    public IList<LinkResponse> _links { get; private set; } = new List<LinkResponse>();
}
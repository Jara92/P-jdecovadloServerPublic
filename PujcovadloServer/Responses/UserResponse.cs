namespace PujcovadloServer.Responses;

public class UserResponse
{
    // TODO: EntityResponse instead of this
    public int Id { get; set; }
    
    public string Username { get; set; } = default!;
    
    public string FirstName { get; set; } = default!;
    
    public string LastName { get; set; } = default!;
    
    public IList<LinkResponse> Links { get; private set; } = new List<LinkResponse>();
}
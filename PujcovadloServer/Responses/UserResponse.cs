namespace PujcovadloServer.Responses;

public class UserResponse
{
    public int Id { get; set; }
    
    public string Username { get; set; } = default!;
    
    public string FirstName { get; set; } = default!;
    
    public string LastName { get; set; } = default!;
}
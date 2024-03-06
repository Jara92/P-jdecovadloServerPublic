namespace PujcovadloServer.Areas.Admin.Responses;

public class UserResponse
{
    public string Id { get; set; }

    public string Username { get; set; }

    public string FirstName { get; set; }

    public string LastName { get; set; }

    public string Name => $"{FirstName} {LastName}";

    public List<string> Roles { get; set; } = new List<string>();

    public DateTime? DateOfBirth { get; set; }

    public DateTime? CreatedAt { get; set; }
}
namespace PujcovadloServer.Business.Objects;

public class ApplicationUserOption
{
    public string Id { get; set; }

    public string FirstName { get; set; }

    public string LastName { get; set; }

    public string Name => $"{FirstName} {LastName}";
}
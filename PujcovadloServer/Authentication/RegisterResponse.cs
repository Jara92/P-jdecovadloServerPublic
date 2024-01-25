using System.ComponentModel.DataAnnotations;

namespace PujcovadloServer.Authentication;

public class RegisterResponse
{
    public string Status { get; set; } = null!;

    public IList<string> Messages { get; set; } = new List<string>();
}
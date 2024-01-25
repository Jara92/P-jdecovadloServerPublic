using System.ComponentModel.DataAnnotations;

namespace PujcovadloServer.Authentication;

public class LoginRequest
{
    [Required] [MaxLength(16)] public string Username { get; set; } = null!;
    
    [Required] [MaxLength(128)] public string Password { get; set; } = null!;
}
using System.ComponentModel.DataAnnotations;

namespace PujcovadloServer.Authentication;

public class RegisterRequest
{
    [Required] [MaxLength(16)] public string Username { get; set; } = null!;
    
    [Required] [MaxLength(32)] public string FirstName { get; set; } = null!;

    [Required] [MaxLength(32)] public string LastName { get; set; } = null!;

    [Required] [MaxLength(64)] [EmailAddress] public string Email { get; set; } = null!;

    [MaxLength(20)] public string? PhoneNumber { get; set; }

    [Required] [MaxLength(128)] public string Password { get; set; } = null!;
    
    [Required] [MaxLength(128)] public string PasswordConfirmation { get; set; } = null!;

    public DateTime? DateOfBirth { get; set; }
}
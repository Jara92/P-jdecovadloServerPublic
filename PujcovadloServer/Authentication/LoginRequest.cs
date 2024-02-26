using System.ComponentModel.DataAnnotations;

namespace PujcovadloServer.Authentication;

public class LoginRequest
{
    [Required(ErrorMessage = "Please enter your {0}.")]
    [Display(Name = "Username")]
    [MaxLength(16)]
    public string Username { get; set; } = null!;

    [Required(ErrorMessage = "Please enter your {0}.")]
    [Display(Name = "Password")]
    [MaxLength(128)]
    public string Password { get; set; } = null!;
}
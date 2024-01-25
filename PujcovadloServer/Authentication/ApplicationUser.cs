using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace PujcovadloServer.Authentication;

public class ApplicationUser : IdentityUser<int>
{
    
    [Required] [MaxLength(32)] public string FirstName { get; set; } = null!;

    [Required] [MaxLength(32)] public string LastName { get; set; } = null!;

    [Required] [MaxLength(64)] public string Email { get; set; } = null!;

    [MaxLength(20)] public string? PhoneNumber { get; set; }

    public DateTime? DateOfBirth { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }
}
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace PujcovadloServer.Authentication;

public class ApplicationUser : IdentityUser<int>
{
    
    [Required] [MaxLength(32)] [Column(TypeName = "VARCHAR")] public string FirstName { get; set; } = null!;

    [Required] [MaxLength(32)] [Column(TypeName = "VARCHAR")] public string LastName { get; set; } = null!;

    public DateTime? DateOfBirth { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }
}
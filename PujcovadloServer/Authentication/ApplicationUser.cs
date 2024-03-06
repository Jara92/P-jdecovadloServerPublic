using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using PujcovadloServer.Business.Entities;

namespace PujcovadloServer.Authentication;

public class ApplicationUser : IdentityUser
{
    [Required]
    [MaxLength(32)]
    [Column(TypeName = "VARCHAR")]
    public string FirstName { get; set; } = null!;

    [Required]
    [MaxLength(32)]
    [Column(TypeName = "VARCHAR")]
    public string LastName { get; set; } = null!;

    public DateTime? DateOfBirth { get; set; }

    public virtual Profile? Profile { get; set; }

    public DateTime? CreatedAt { get; set; } = DateTime.Now;

    public DateTime? UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }
}
using System.ComponentModel.DataAnnotations;
using PujcovadloServer.Attributes;

namespace PujcovadloServer.Areas.Admin.Requests;

public class UserRequest
{
    public string? Id { get; set; }

    [Required]
    [StringLength(32, MinimumLength = 4)]
    [RegularExpression(@"^[a-zA-Z0-9_]*$", ErrorMessage = "Only letters, numbers, and underscores are allowed.")]
    public string Username { get; set; }

    [Required]
    [StringLength(32, MinimumLength = 4)]
    public string FirstName { get; set; }

    [Required]
    [StringLength(32, MinimumLength = 4)]
    public string LastName { get; set; }

    public string Name => $"{FirstName} {LastName}";

    [UserRoles] public IList<string> Roles { get; set; } = new List<string>();

    public DateTime? DateOfBirth { get; set; }

    [DataType(DataType.DateTime)] public DateTime? CreatedAt { get; set; }
}
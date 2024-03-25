using System.ComponentModel.DataAnnotations;

namespace PujcovadloServer.Areas.Admin.Requests;

public class ItemTagRequest
{
    public int? Id { get; set; }

    [Required]
    [StringLength(40, MinimumLength = 4)]
    public string Name { get; set; }

    [Required] public bool IsApproved { get; set; }
}
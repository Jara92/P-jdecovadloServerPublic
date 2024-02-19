using System.ComponentModel.DataAnnotations;

namespace PujcovadloServer.Requests;

public class ItemTagRequest : EntityRequest
{
    [Required]
    [StringLength(20, MinimumLength = 4)]
    public string Name { get; set; } = null!;
}
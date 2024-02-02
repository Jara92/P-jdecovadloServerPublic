using System.ComponentModel.DataAnnotations;

namespace PujcovadloServer.Requests;

public class ItemTagRequest : EntityRequest
{
    [Required]
    [MaxLength(20)]
    public string Name { get; set; } = null!;
}
using System.ComponentModel.DataAnnotations;

namespace PujcovadloServer.Requests;

public class ItemCategoryRequest : EntityRequest
{
    [Required]
    [StringLength(32, MinimumLength = 4)]
    public string Name { get; set; } = default!;

    [StringLength(40, MinimumLength = 4)] public string? Alias { get; set; }

    [StringLength(512, MinimumLength = 0)] public string? Description { get; set; }
}
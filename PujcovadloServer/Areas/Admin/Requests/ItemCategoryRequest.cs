using System.ComponentModel.DataAnnotations;
using PujcovadloServer.Attributes;

namespace PujcovadloServer.Areas.Admin.Requests;

public class ItemCategoryRequest
{
    public int? Id { get; set; } = null!;

    [Required]
    [StringLength(32, MinimumLength = 4)]
    public string Name { get; set; } = default!;

    [StringLength(32)] public string? Alias { get; set; }

    public string? Description { get; set; } = default!;

    [NotEqualTo(nameof(Id), ErrorMessage = "Parent category cannot be the category itself.")]
    public int? ParentId { get; set; }
}
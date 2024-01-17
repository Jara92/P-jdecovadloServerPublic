using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace PujcovadloServer.Models;

public class ItemCategory : BaseEntity
{
    [Required]
    [StringLength(32, MinimumLength = 4)]
    public string Name { get; set; } = default!;
    
    [ReadOnly(true)]
    public string? Alias { get; set; }
    
    public string Description { get; set; } = default!;
    
    public virtual ItemCategory? Parent { get; set; }

    [JsonIgnore] public virtual ICollection<Item> Items { get; } = new List<Item>();
}
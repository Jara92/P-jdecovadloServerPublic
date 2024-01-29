using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace PujcovadloServer.Business.Entities;

public class ItemCategory : BaseEntity
{
    [Required]
    [Column(TypeName = "VARCHAR")]
    [StringLength(32, MinimumLength = 4)]
    public string Name { get; set; } = default!;
    
    [Column(TypeName = "VARCHAR")]
    [StringLength(32, MinimumLength = 1)]
    public string? Alias { get; set; } 
    
    public string Description { get; set; } = default!;
    
    public virtual ItemCategory? Parent { get; set; }

    [JsonIgnore] public virtual ICollection<Item> Items { get; } = new List<Item>();
}
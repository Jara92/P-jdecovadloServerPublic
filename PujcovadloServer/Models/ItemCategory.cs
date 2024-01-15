using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace PujcovadloServer.Models;

public class ItemCategory
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(32, MinimumLength = 4)]
    public string Name { get; set; } = "";
    
    [ReadOnly(true)]
    public string Alias { get; set; } = "";
    
    public string Description { get; set; } = "";
    
    public virtual ItemCategory? Parent { get; set; }

    [JsonIgnore] public virtual ICollection<Item> Items { get; } = new List<Item>();
}
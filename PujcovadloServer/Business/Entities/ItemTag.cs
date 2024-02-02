using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace PujcovadloServer.Business.Entities;

public class ItemTag : BaseEntity
{
    [Required]
    [Column(TypeName = "VARCHAR")]
    [StringLength(32, MinimumLength = 4)]
    public string Name { get; set; } = default!;

    public bool IsApproved { get; set; } = false;

    public virtual ICollection<Item> Items { get; set; } = new List<Item>();
}
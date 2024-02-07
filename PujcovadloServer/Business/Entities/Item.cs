using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PujcovadloServer.Authentication;
using PujcovadloServer.Business.Enums;

namespace PujcovadloServer.Business.Entities;

public class Item : BaseEntity
{
    [Required]
    [Column(TypeName = "VARCHAR")]
    [StringLength(64, MinimumLength = 6)]
    [RegularExpression(@"^[A-Z]+[a-zA-Z]+[0-9]*$")]
    public string Name { get; set; } = default!;
    
    [Column(TypeName = "VARCHAR")]
    [StringLength(64, MinimumLength = 1)]
    public string? Alias { get; set; }

    public string Description { get; set; } = default!;
    
    [ReadOnly(true)]
    public ItemStatus Status { get; set; } = ItemStatus.Public;
    
    public string Parameters { get; set; } = "";
    
    [Required] 
    public float PricePerDay { get; set; }
    
    public float? RefundableDeposit { get; set; }
    
    public float? PurchasePrice { get; set; }
    
    public float? SellingPrice { get; set; }

    public virtual ICollection<ItemCategory> Categories { get; } = new List<ItemCategory>();
    
    public virtual ICollection<ItemTag> Tags { get; } = new List<ItemTag>();
    
    public virtual IList<Image> Images { get; set; } = new List<Image>();
    
    public virtual ApplicationUser Owner { get; set; } = default!;
    
    [ReadOnly(true)]
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    
    [ReadOnly(true)]
    public DateTime? UpdatedAt { get; set; }
    
    [ReadOnly(true)]
    public DateTime? ApprovedAt { get; set; }

    [ReadOnly(true)] public DateTime? DeletedAt { get; set; }
}
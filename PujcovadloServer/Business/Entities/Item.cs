using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using PujcovadloServer.Authentication;
using PujcovadloServer.Business.Enums;

namespace PujcovadloServer.Business.Entities;

public class Item : BaseEntity
{
    [Required]
    [StringLength(64, MinimumLength = 6)]
    [RegularExpression(@"^[A-Z]+[a-zA-Z]+[0-9]*$")]
    public string Name { get; set; }
    
    [StringLength(64, MinimumLength = 1)]
    public string? Alias { get; set; }
    
    public string Description { get; set; }
    
    [ReadOnly(true)]
    public ItemStatus Status { get; set; } = ItemStatus.Public;
    
    public string Parameters { get; set; } = "";
    
    [Required] 
    public float PricePerDay { get; set; }
    
    public float? RefundableDeposit { get; set; }
    
    public float? PurchasePrice { get; set; }
    
    public float? SellingPrice { get; set; }

    public virtual ICollection<ItemCategory> Categories { get; } = new List<ItemCategory>();
    
    public virtual ApplicationUser Owner { get; set; } = default!;
    
    [ReadOnly(true)]
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    
    [ReadOnly(true)]
    public DateTime? UpdatedAt { get; set; }
    
    [ReadOnly(true)]
    public DateTime? ApprovedAt { get; set; }

    [ReadOnly(true)] public DateTime? DeletedAt { get; set; }
}
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
    [DisplayName("Name")]
    public string Name { get; set; } = default!;

    [Column(TypeName = "VARCHAR")]
    [StringLength(64, MinimumLength = 1)]
    [DisplayName("Alias")]
    public string? Alias { get; set; }

    [DisplayName("Description")] public string Description { get; set; } = default!;

    [ReadOnly(true)]
    [DisplayName("Status")]
    public ItemStatus Status { get; set; } = ItemStatus.Public;

    [DisplayName("Parameters")] public string Parameters { get; set; } = "";

    [Required]
    [DisplayName("Price per day")]
    public float PricePerDay { get; set; }

    [DisplayName("Refundable deposit")] public float? RefundableDeposit { get; set; }

    [DisplayName("Purchase price")] public float? PurchasePrice { get; set; }

    [DisplayName("Selling price")] public float? SellingPrice { get; set; }


    public virtual ICollection<ItemCategory> Categories { get; } = new List<ItemCategory>();

    public virtual ICollection<ItemTag> Tags { get; } = new List<ItemTag>();

    public int? MainImageId { get; set; }

    [DisplayName("Main image")] public virtual Image? MainImage { get; set; }

    public virtual IList<Image> Images { get; set; } = new List<Image>();

    public string OwnerId { get; set; } = default!;

    [DisplayName("Owner")] public virtual ApplicationUser Owner { get; set; } = default!;

    [DisplayName("Created at")]
    [ReadOnly(true)]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [DisplayName("Updated at")]
    [ReadOnly(true)]
    public DateTime? UpdatedAt { get; set; }

    [DisplayName("Approved at")]
    [ReadOnly(true)]
    public DateTime? ApprovedAt { get; set; }

    [DisplayName("Deleted at")]
    [ReadOnly(true)]
    public DateTime? DeletedAt { get; set; }
}
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PujcovadloServer.Attributes;
using PujcovadloServer.Business.Enums;

namespace PujcovadloServer.Areas.Admin.Requests;

public class ItemRequest
{
    [DisplayName("Id")] public int? Id { get; set; }

    [Required]
    [Column(TypeName = "VARCHAR")]
    [StringLength(64, MinimumLength = 6)]
    // TODO: Add RegularExpression
    /*[RegularExpression(@"^[A-Z]+[a-zA-Z]+[0-9]*$")]*/
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

    [DisplayName("Parameters")] public string? Parameters { get; set; } = "";

    [Required]
    [DisplayName("Price per day")]
    public float PricePerDay { get; set; }

    [DisplayName("Refundable deposit")] public float? RefundableDeposit { get; set; }

    [DisplayName("Purchase price")] public float? PurchasePrice { get; set; }

    [DisplayName("Selling price")] public float? SellingPrice { get; set; }

    [ItemCategoryExists]
    [DisplayName("Categories")]
    public ICollection<int> Categories { get; } = new List<int>();

    [DisplayName("Tags")] public ICollection<int> Tags { get; } = new List<int>();

    [DisplayName("Owner")] public string OwnerId { get; set; } = default!;

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
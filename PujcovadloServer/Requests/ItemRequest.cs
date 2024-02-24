using System.ComponentModel.DataAnnotations;
using PujcovadloServer.Areas.Api.Attributes;

namespace PujcovadloServer.Requests;

public class ItemRequest : EntityRequest
{
    [Required]
    [StringLength(64, MinimumLength = 6)]
    public string Name { get; set; } = default!;

    [Required]
    [StringLength(1024, MinimumLength = 0)]
    public string Description { get; set; } = default!;

    [Required]
    [StringLength(256, MinimumLength = 0)]
    public string Parameters { get; set; } = "";

    [Required] [Price(0, 1000000)] public float PricePerDay { get; set; }

    [Price(0, 10000000)] public float? RefundableDeposit { get; set; }

    [Price(0, 10000000)] public float? SellingPrice { get; set; }

    [Price(0, 10000000)] public float? PurchasePrice { get; set; }

    [MaxLength(5)] public virtual ICollection<int> Categories { get; set; } = new List<int>();

    [MaxLength(10)] public virtual ICollection<string> Tags { get; set; } = new List<string>();

    public int? MainImageId { get; set; }
}
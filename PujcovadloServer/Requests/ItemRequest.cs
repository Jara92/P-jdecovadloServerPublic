using System.ComponentModel.DataAnnotations;
using PujcovadloServer.Areas.Api.Attributes;
using PujcovadloServer.Business.Enums;

namespace PujcovadloServer.Requests;

public class ItemRequest : EntityRequest
{
    [Required]
    [StringLength(64, MinimumLength = 6)]
    public string Name { get; set; } = default!;

    [Required]
    [StringLength(1024, MinimumLength = 0)]
    public string Description { get; set; } = default!;

    // todo: this is not a good way to represent parameters so this field is not required not
    /*[Required]*/
    [StringLength(256, MinimumLength = 0)] public string Parameters { get; set; } = "";

    [Required] [Price(0, 1000000)] public float? PricePerDay { get; set; }

    [Price(0, 10000000)] public float? RefundableDeposit { get; set; }

    [Price(0, 10000000)] public float? SellingPrice { get; set; }

    [Price(0, 10000000)] public float? PurchasePrice { get; set; }

    public ItemStatus? Status { get; set; }

    [MaxLength(5)] public virtual IList<int> Categories { get; set; } = new List<int>();

    [MaxLength(10)] [ItemsLength(0, 40)] public virtual IList<string> Tags { get; set; } = new List<string>();

    public int? MainImageId { get; set; }

    public decimal? Latitude { get; set; }

    public decimal? Longitude { get; set; }
}
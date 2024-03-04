using PujcovadloServer.Business.Enums;
using PujcovadloServer.Responses;

namespace PujcovadloServer.Areas.Admin.Responses;

public class ItemResponse
{
    public int Id { get; set; }

    public string Name { get; set; } = default!;

    public string? Alias { get; set; } = default!;

    public string Description { get; set; } = default!;

    public ItemStatus Status { get; set; }

    public float PricePerDay { get; set; }

    public float? RefundableDeposit { get; set; }

    public float? SellingPrice { get; set; }

    public UserResponse Owner { get; set; } = default!;

    public string OwnerId => Owner.Id;

    public ImageResponse? MainImage { get; set; } = default!;

    public int? MainImageId => MainImage?.Id;

    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
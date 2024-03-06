using PujcovadloServer.Business.Enums;
using PujcovadloServer.Responses;

namespace PujcovadloServer.Areas.Admin.Responses;

public class LoanResponse
{
    public int Id { get; set; }

    public LoanStatus? Status { get; set; }

    public DateTime? From { get; set; }

    public DateTime? To { get; set; }

    public int Days { get; set; }

    public float PricePerDay { get; set; }

    public float? RefundableDeposit { get; set; }

    public float ExpectedPrice { get; set; }

    public string? TenantNote { get; set; }

    public int ItemId { get; set; }

    public ItemResponse Item { get; set; } = default!;

    public UserResponse Tenant { get; set; } = default!;

    public string TenantId { get; set; } = default!;

    public string OwnerId => Item.OwnerId;

    public int? PickupProtocolId { get; set; }

    public int? ReturnProtocolId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
using PujcovadloServer.Authentication;
using PujcovadloServer.Business.Enums;

namespace PujcovadloServer.Business.Entities;

public class Loan : BaseEntity
{
    public LoanStatus Status { get; set; } = LoanStatus.Inquired;

    public DateTime From { get; set; }

    public DateTime To { get; set; }

    public int Days { get; set; }

    public float PricePerDay { get; set; }

    public float ExpectedPrice { get; set; }

    public float? RefundableDeposit { get; set; }

    public string? TenantNote { get; set; }

    public virtual ApplicationUser Tenant { get; set; } = default!;

    public virtual Item Item { get; set; } = default!;

    public int? PickupProtocolId { get; set; }

    public virtual PickupProtocol? PickupProtocol { get; set; }

    public int? ReturnProtocolId { get; set; }

    public virtual ReturnProtocol? ReturnProtocol { get; set; }

    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public DateTime? UpdatedAt { get; set; }
}
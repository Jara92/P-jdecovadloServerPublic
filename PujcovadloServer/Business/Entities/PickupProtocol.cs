namespace PujcovadloServer.Business.Entities;

public class PickupProtocol : BaseEntity
{
    public string? Description { get; set; }

    public DateTime? ConfirmedAt { get; set; }

    public float? AcceptedRefundableDeposit { get; set; }

    public virtual Loan Loan { get; set; } = default!;

    public virtual IList<Image> Images { get; set; } = new List<Image>();

    public DateTime? CreatedAt { get; set; } = DateTime.Now;

    public DateTime? UpdatedAt { get; set; }
}
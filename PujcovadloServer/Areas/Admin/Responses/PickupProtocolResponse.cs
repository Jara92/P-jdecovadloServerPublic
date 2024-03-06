namespace PujcovadloServer.Areas.Admin.Responses;

public class PickupProtocolResponse
{
    public int Id { get; set; }

    public string? Description { get; set; }

    public DateTime? ConfirmedAt { get; set; }

    public float? AcceptedRefundableDeposit { get; set; }

    public int? LoanId { get; set; }

    public virtual LoanResponse Loan { get; set; } = default!;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
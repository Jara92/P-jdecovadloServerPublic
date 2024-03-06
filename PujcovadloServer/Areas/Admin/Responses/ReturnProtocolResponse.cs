namespace PujcovadloServer.Areas.Admin.Responses;

public class ReturnProtocolResponse
{
    public int Id { get; set; }

    public string? Description { get; set; }

    public DateTime? ConfirmedAt { get; set; }

    public float? ReturnedRefundableDeposit { get; set; }

    public int? LoanId { get; set; }

    public virtual LoanResponse Loan { get; set; } = default!;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
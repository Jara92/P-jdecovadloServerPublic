namespace PujcovadloServer.Requests;

public class PickupProtocolRequest
{
    public string? Description { get; set; }
    
    public float? AcceptedRefundableDeposit { get; set; } = null!;
    
    // TODO: add common LoanRequest properties
    public virtual TenantLoanRequest Loan { get; set; } = default!;
}
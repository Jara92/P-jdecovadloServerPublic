namespace PujcovadloServer.Requests;

public class ReturnProtocolRequest
{
    public string? Description { get; set; }

    public float? ReturnedRefundableDeposit { get; set; } = null!;

    // TODO: add common LoanRequest properties
}
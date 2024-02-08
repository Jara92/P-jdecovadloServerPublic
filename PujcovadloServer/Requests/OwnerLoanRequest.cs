using PujcovadloServer.Business.Enums;

namespace PujcovadloServer.Requests;

public class OwnerLoanRequest : EntityRequest
{
    public LoanStatus? Status { get; set; } = null!;
}
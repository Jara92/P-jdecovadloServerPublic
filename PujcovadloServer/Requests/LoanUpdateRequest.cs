using PujcovadloServer.Business.Enums;

namespace PujcovadloServer.Requests;

public class LoanUpdateRequest : EntityRequest
{
    public LoanStatus? Status { get; set; } = null!;

    public string? TenantNote { get; set; }
}
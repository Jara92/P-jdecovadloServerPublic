using System.ComponentModel;

namespace PujcovadloServer.Business.Filters;

public class LoanFilter : BaseFilter
{
    [ReadOnly(true)]
    public string? TenantId { get; set; }
    
    [ReadOnly(true)]
    public string? OwnerId { get; set; }
}
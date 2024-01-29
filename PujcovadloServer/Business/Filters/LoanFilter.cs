using System.ComponentModel;

namespace PujcovadloServer.Business.Filters;

public class LoanFilter : BaseFilter
{
    [ReadOnly(true)]
    public int? TenantId { get; set; }
}
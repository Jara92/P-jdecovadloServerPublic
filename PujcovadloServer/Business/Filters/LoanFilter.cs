using System.ComponentModel;

namespace PujcovadloServer.Business.Filters;

public class LoanFilter : BaseFilter
{
    public LoanFilter()
    {
    }

    public LoanFilter(LoanFilter filter) : base(filter)
    {
        TenantId = filter.TenantId;
        OwnerId = filter.OwnerId;
    }


    public override BaseFilter Clone()
    {
        return new LoanFilter(this);
    }

    [ReadOnly(true)] public string? TenantId { get; set; }

    [ReadOnly(true)] public string? OwnerId { get; set; }
}
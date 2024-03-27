namespace PujcovadloServer.Business.Filters;

public class LoanFilter : BaseFilter
{
    public LoanFilter()
    {
    }

    public LoanFilter(LoanFilter filter) : base(filter)
    {
        Search = filter.Search;
        TenantId = filter.TenantId;
        OwnerId = filter.OwnerId;
    }


    public override BaseFilter Clone()
    {
        return new LoanFilter(this);
    }

    public string? Search { get; set; }

    public string? TenantId { get; set; }

    public string? OwnerId { get; set; }
}
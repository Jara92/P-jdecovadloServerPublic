namespace PujcovadloServer.Business.Filters;

public class ItemTagFilter : BaseFilter
{
    public ItemTagFilter()
    {
    }

    public ItemTagFilter(ItemTagFilter filter) : base(filter)
    {
        Search = filter.Search;
        OnlyApproved = filter.OnlyApproved;
    }

    public override ItemTagFilter Clone()
    {
        return new ItemTagFilter(this);
    }

    public string? Search { get; set; }

    public bool? OnlyApproved { get; set; }
}
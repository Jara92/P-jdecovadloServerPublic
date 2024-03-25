namespace PujcovadloServer.Business.Filters;

public class ItemCategoryFilter : BaseFilter
{
    public ItemCategoryFilter()
    {
        // todo: find better way - 100 is global page maximum but categories should be loaded all at once
        PageSize = 100;
    }

    public ItemCategoryFilter(ItemCategoryFilter filter) : base(filter)
    {
    }

    public override ItemCategoryFilter Clone()
    {
        return new ItemCategoryFilter(this);
    }
}
namespace PujcovadloServer.Business.Filters;

public class ItemCategoryFilter : BaseFilter
{
    public ItemCategoryFilter()
    {
    }
    
    public ItemCategoryFilter(ItemCategoryFilter filter) : base(filter)
    {
       
    }

    public ItemCategoryFilter Clone()
    {
        return new ItemCategoryFilter(this);
    }
    
    
}
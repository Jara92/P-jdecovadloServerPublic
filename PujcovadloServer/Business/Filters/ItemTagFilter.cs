namespace PujcovadloServer.Business.Filters;

public class ItemTagFilter : BaseFilter
{
    public ItemTagFilter()
    {
    }
    
    public ItemTagFilter(ItemTagFilter filter) : base(filter)
    {
       
    }

    public ItemTagFilter Clone()
    {
        return new ItemTagFilter(this);
    }
    
    
}
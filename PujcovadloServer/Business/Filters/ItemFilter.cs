using PujcovadloServer.Business.Entities;
using PujcovadloServer.Business.Enums;

namespace PujcovadloServer.Business.Filters;

public class ItemFilter : BaseFilter
{
    public ItemFilter()
    {
    }
    
    public ItemFilter(ItemFilter filter) : base(filter)
    {
        Search = filter.Search;
        Status = filter.Status;
        Category = filter.Category;
    }

    public ItemFilter Clone()
    {
        return new ItemFilter(this);
    }
    
    public string? Search { get; set; }
    
    public ItemStatus? Status { get; set; } 
    
    public ItemCategory? Category { get; set; }
}
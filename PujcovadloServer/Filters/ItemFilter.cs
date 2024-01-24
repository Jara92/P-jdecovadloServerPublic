using PujcovadloServer.Enums;
using PujcovadloServer.Models;

namespace PujcovadloServer.Filters;

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
    
    public string? Search { get; set; }
    
    public ItemStatus? Status { get; set; } 
    
    public ItemCategory? Category { get; set; }
}
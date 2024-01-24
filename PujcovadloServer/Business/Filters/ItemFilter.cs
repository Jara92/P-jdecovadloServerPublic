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
        CategoryId = filter.CategoryId;
    }

    public ItemFilter Clone()
    {
        return new ItemFilter(this);
    }
    
    /// <summary>
    /// Search string to search in item name and description
    /// </summary>
    public string? Search { get; set; }
    
    /// <summary>
    /// Item status.
    /// </summary>
    public ItemStatus? Status { get; set; } 
    
    /// <summary>
    /// Item category.
    /// </summary>
    public int? CategoryId { get; set; }
}
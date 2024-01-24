using PujcovadloServer.Enums;

namespace PujcovadloServer.Filters;

public class BaseFilter
{
    public BaseFilter()
    {
    }
    
    public BaseFilter(BaseFilter filter)
    {
        Page = filter.Page;
        PageSize = filter.PageSize;
        Sortby = filter.Sortby;
        SortOrder = filter.SortOrder;
    }
    
    public int Page { get; set; } = 1;
    
    public int PageSize { get; set; } = 2;
    
    public string? Sortby { get; set; } = "Id";
    
    public bool? SortOrder { get; set; } = true;
}
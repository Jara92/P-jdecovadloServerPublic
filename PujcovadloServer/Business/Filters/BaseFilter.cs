using System.ComponentModel.DataAnnotations;

namespace PujcovadloServer.Business.Filters;

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

    public BaseFilter Clone()
    {
        return new BaseFilter(this);
    }

    /// <summary>
    /// Page number.
    /// </summary>
    public int Page { get; set; } = 1;

    /// <summary>
    /// Page size.
    /// </summary>
    [Range(1, 100)]
    public virtual int PageSize { get; set; } = 2;

    /// <summary>
    /// Column to sort the result by.
    /// </summary>
    public string? Sortby { get; set; } = "Id";

    /// <summary>
    /// Sort direction.
    /// True = ascending, false = descending.
    /// </summary>
    public bool? SortOrder { get; set; } = true;
}
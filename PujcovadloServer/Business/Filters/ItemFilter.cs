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
        OwnerId = filter.OwnerId;
        Latitude = filter.Latitude;
        Longitude = filter.Longitude;
    }


    public override BaseFilter Clone()
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

    /// <summary>
    /// Owner (user) id.
    /// </summary>
    public string? OwnerId { get; set; }

    public float? Latitude { get; set; }

    public float? Longitude { get; set; }
}
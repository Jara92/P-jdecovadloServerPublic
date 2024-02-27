namespace PujcovadloServer.Areas.Admin.Business.Filters;

public class ItemFilter : PujcovadloServer.Business.Filters.ItemFilter
{
    public override int PageSize { get; set; } = 10;
}
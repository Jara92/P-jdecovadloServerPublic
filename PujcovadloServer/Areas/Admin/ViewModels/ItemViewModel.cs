using PujcovadloServer.Business.Entities;
using PujcovadloServer.Business.Filters;
using X.PagedList;

namespace PujcovadloServer.Areas.Admin.ViewModels;

public class ItemViewModel
{
    public IPagedList<Item> Items { get; set; } = default!;

    public ItemFilter Filter { get; set; } = default!;
}
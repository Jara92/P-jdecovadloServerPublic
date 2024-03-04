using System.Collections;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using PujcovadloServer.Business.Entities;
using PujcovadloServer.Business.Enums;
using PujcovadloServer.Data;
using Syncfusion.EJ2.Base;

namespace PujcovadloServer.Areas.Admin.Services;

public class ItemService
{
    private readonly PujcovadloServerContext _dbContext;
    private readonly IStringLocalizer<ItemStatus> _itemStatusLocalizer;

    public ItemService(PujcovadloServerContext dbContext, IStringLocalizer<ItemStatus> itemStatusLocalizer)
    {
        _dbContext = dbContext;
        _itemStatusLocalizer = itemStatusLocalizer;
    }

    public Task<List<object>> GetItemStatusOptions()
    {
        var statuses = new List<object>();
        foreach (var i in Enum.GetValues(typeof(ItemStatus)))
        {
            statuses.Add(new
            {
                Id = i,
                // Translate the status name
                Name = _itemStatusLocalizer[i.ToString()].Value
            });
        }

        return Task.FromResult(statuses);
    }

    public Task<List<Item>> GetItems(DataManagerRequest dm)
    {
        var query = GetFilteredQuery(dm);
        var operations = new DataOperations();

        if (dm.Skip != 0)
        {
            query = operations.PerformSkip(query, dm.Skip); //Paging
        }

        if (dm.Take != 0)
        {
            query = operations.PerformTake(query, dm.Take);
        }

        return query.ToListAsync();
    }

    public Task<IEnumerable> GetAggregations(DataManagerRequest dm)
    {
        var query = GetFilteredQuery(dm);

        List<string> str = new List<string>();
        if (dm.Aggregates != null)
        {
            for (var i = 0; i < dm.Aggregates.Count; i++)
                str.Add(dm.Aggregates[i].Field);
        }

        var operations = new DataOperations();

        IEnumerable aggregate = operations.PerformSelect(query, str);

        return Task.FromResult(aggregate);
    }

    public Task<int> GetItemsCount(DataManagerRequest dm)
    {
        return GetFilteredQuery(dm).CountAsync();
    }

    private IQueryable<Item> GetFilteredQuery(DataManagerRequest dm)
    {
        var query = _dbContext.Item.AsNoTracking().AsQueryable();

        var operations = new DataOperations();

        if (dm.Search != null && dm.Search.Count > 0)
        {
            query = operations.PerformSearching(query, dm.Search); //Search
        }

        if (dm.Sorted != null && dm.Sorted.Count > 0) //Sorting
        {
            query = operations.PerformSorting(query, dm.Sorted);
        }

        if (dm.Where != null && dm.Where.Count > 0) //Filtering
        {
            query = operations.PerformFiltering(query, dm.Where, dm.Where[0].Operator);
        }

        return query;
    }
}
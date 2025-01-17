using PujcovadloServer.Business.Entities;
using PujcovadloServer.Business.Enums;
using PujcovadloServer.Business.Filters;
using PujcovadloServer.Business.Interfaces;
using PujcovadloServer.Business.Objects;
using PujcovadloServer.Lib;

namespace PujcovadloServer.Business.Services;

public class ItemService(IItemRepository repository) : ACrudService<Item, IItemRepository, ItemFilter>(repository)
{
    public override async Task<PaginatedList<Item>> GetAll(ItemFilter filter)
    {
        return await _repository.GetAll(filter);
    }

    public virtual async Task<PaginatedList<Item>> GetAllPublic(ItemFilter filter)
    {
        filter.Status = ItemStatus.Public;
        return await GetAll(filter);
    }

    public override async Task Update(Item item)
    {
        item.UpdatedAt = DateTime.Now;
        await base.Update(item);
    }

    public override Task Create(Item entity)
    {
        entity.CreatedAt = DateTime.Now;
        return base.Create(entity);
    }

    public override Task Delete(Item entity)
    {
        // todo add option for soft / hard deltee

        entity.DeletedAt = DateTime.Now;
        entity.Status = ItemStatus.Deleted;

        // Just update the item
        return base.Update(entity);
    }

    public async Task<int> GetPublicItemsCountByUser(string userId)
    {
        return await _repository.GetPublicItemsCountByUser(userId);
    }

    public Task<List<EntityOption>> GetItemStatusOptions()
    {
        var statuses = new List<EntityOption>();
        foreach (var i in Enum.GetValues(typeof(ItemStatus)))
        {
            statuses.Add(new EntityOption()
            {
                Id = (int)i,
                Name = i.ToString(),
            });
        }

        return Task.FromResult(statuses);
    }
}
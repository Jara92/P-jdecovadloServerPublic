using PujcovadloServer.Business.Entities;
using PujcovadloServer.Business.Enums;
using PujcovadloServer.Business.Filters;
using PujcovadloServer.Business.Interfaces;
using PujcovadloServer.Lib;

namespace PujcovadloServer.Business.Services;

public class ItemService(IItemRepository repository) : ACrudService<Item, IItemRepository, ItemFilter>(repository)
{
    public override async Task<PaginatedList<Item>> GetAll(ItemFilter filter)
    {
        return await _repository.GetAll(filter);
    }

    public async Task<PaginatedList<Item>> GetAllPublic(ItemFilter filter)
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
        entity.DeletedAt = DateTime.Now;
        entity.Status = ItemStatus.Deleted;

        return base.Delete(entity);
    }
}
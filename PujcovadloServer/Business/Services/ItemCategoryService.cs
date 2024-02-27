using PujcovadloServer.Business.Entities;
using PujcovadloServer.Business.Filters;
using PujcovadloServer.Business.Interfaces;
using PujcovadloServer.Business.Objects;

namespace PujcovadloServer.Business.Services;

public class ItemCategoryService : ACrudService<ItemCategory, IItemCategoryRepository, ItemCategoryFilter>
{
    public ItemCategoryService(IItemCategoryRepository repository) : base(repository)
    {
    }

    public virtual async Task<IList<ItemCategory>> GetByIds(IEnumerable<int> ids)
    {
        return await _repository.GetByIds(ids);
    }

    public Task<IList<ItemCategoryOption>> GetAllOptions()
    {
        return _repository.GetAllOptions();
    }
}
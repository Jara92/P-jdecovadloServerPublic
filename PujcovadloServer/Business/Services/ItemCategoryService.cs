using PujcovadloServer.Business.Entities;
using PujcovadloServer.Business.Filters;
using PujcovadloServer.Business.Interfaces;

namespace PujcovadloServer.Business.Services;

public class ItemCategoryService : ACrudService<ItemCategory, ItemCategoryFilter>
{
    protected IItemCategoryRepository _repository;
    
    public ItemCategoryService(IItemCategoryRepository repository) : base(repository)
    {
        _repository = repository;
    }
    
    public async Task<IList<ItemCategory>> GetByIds(IEnumerable<int> ids)
    {
        return await _repository.GetByIds(ids);
    }
}
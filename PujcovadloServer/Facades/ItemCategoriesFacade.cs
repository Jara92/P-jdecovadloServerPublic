using PujcovadloServer.Helpers;
using PujcovadloServer.Models;
using PujcovadloServer.Repositories.Interfaces;

namespace PujcovadloServer.Facades;

public class ItemCategoriesFacade
{
    private readonly IItemCategoryRepository _itemCategoriesRepository;

    public ItemCategoriesFacade(IItemCategoryRepository itemCategoriesRepository)
    {
        _itemCategoriesRepository = itemCategoriesRepository;
    }

    public async Task<ItemCategory?> Get(int id)
    {
        return await _itemCategoriesRepository.Get(id);
    }

    public async Task Create(ItemCategory newCategory)
    {
        newCategory.Alias = UrlHelper.CreateUrlStub(newCategory.Name);
        
        await _itemCategoriesRepository.Create(newCategory);
    }

    public async Task Update(ItemCategory category)
    {
        category.Alias = UrlHelper.CreateUrlStub(category.Name);
        
        await _itemCategoriesRepository.Update(category);
    }
}
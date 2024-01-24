using PujcovadloServer.Business.Entities;
using PujcovadloServer.Business.Services;
using PujcovadloServer.Helpers;

namespace PujcovadloServer.Business.Facades;

public class ItemCategoryFacade
{
    private readonly ItemCategoryService _itemCategoryService;

    public ItemCategoryFacade(ItemCategoryService itemCategoryService)
    {
        _itemCategoryService = itemCategoryService;
    }

    public async Task<ItemCategory?> Get(int id)
    {
        return await _itemCategoryService.Get(id);
    }

    public async Task Create(ItemCategory newCategory)
    {
        newCategory.Alias = UrlHelper.CreateUrlStub(newCategory.Name);
        
        await _itemCategoryService.Create(newCategory);
    }

    public async Task Update(ItemCategory category)
    {
        category.Alias = UrlHelper.CreateUrlStub(category.Name);
        
        await _itemCategoryService.Update(category);
    }
}
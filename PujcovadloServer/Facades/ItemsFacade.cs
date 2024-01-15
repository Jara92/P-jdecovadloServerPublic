using PujcovadloServer.Models;
using PujcovadloServer.Repositories;
using PujcovadloServer.Services;

namespace PujcovadloServer.Facades;

public class ItemsFacade
{
    private readonly ItemsRepository _itemsRepository;
    
    public ItemsFacade(ItemsRepository _itemsRepository)
    {
        this._itemsRepository = _itemsRepository;
    }
    
    public async Task CreateItem(Item item)
    {
        item.Alias = UrlHelper.CreateUrlStub(item.Name);
        
         await _itemsRepository.Create(item);
    }
    
    public async Task UpdateItem(Item item)
    {
        item.Alias = UrlHelper.CreateUrlStub(item.Name);
        
        await _itemsRepository.Update(item);
    }

    public async Task AddCategory(Item item, ItemCategory dbCategory)
    {
        // Check if item already has this category.
        if (! item.Categories.Any(c => c.Id == dbCategory.Id))
        {
            item.Categories.Add(dbCategory);
            await _itemsRepository.Update(item);
        }
    }
}
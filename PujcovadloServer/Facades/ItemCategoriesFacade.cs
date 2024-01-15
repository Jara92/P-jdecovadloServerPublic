using PujcovadloServer.Models;
using PujcovadloServer.Repositories;

namespace PujcovadloServer.Facades;

public class ItemCategoriesFacade
{
    private readonly ItemCategoriesRepository _itemCategoriesRepository;

    public ItemCategoriesFacade(ItemCategoriesRepository itemCategoriesRepository)
    {
        _itemCategoriesRepository = itemCategoriesRepository;
    }

    public async Task<ItemCategory?> Get(int id)
    {
        return await _itemCategoriesRepository.Get(id);
    }
}
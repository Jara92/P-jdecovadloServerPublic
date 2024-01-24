using PujcovadloServer.Filters;
using PujcovadloServer.Models;

namespace PujcovadloServer.Repositories.Interfaces;

public interface IItemCategoryRepository : ICrudRepository<ItemCategory, ItemCategoryFilter>
{
    
}
using PujcovadloServer.Business.Entities;
using PujcovadloServer.Business.Filters;

namespace PujcovadloServer.Business.Interfaces;

public interface IItemCategoryRepository : ICrudRepository<ItemCategory, ItemCategoryFilter>
{
    
}
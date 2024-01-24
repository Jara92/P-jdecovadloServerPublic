using PujcovadloServer.Business.Entities;
using PujcovadloServer.Business.Filters;
using PujcovadloServer.Business.Interfaces;

namespace PujcovadloServer.Data.Repositories;

public class ItemCategoryRepository : ACrudRepository<ItemCategory, ItemCategoryFilter>, IItemCategoryRepository
{
    public ItemCategoryRepository(PujcovadloServerContext context) : base(context)
    {
    }
}
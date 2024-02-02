using PujcovadloServer.Business.Entities;
using PujcovadloServer.Business.Filters;

namespace PujcovadloServer.Business.Interfaces;

public interface IItemTagRepository : ICrudRepository<ItemTag, ItemTagFilter>
{
    public Task<ItemTag?> GetByName(string name);
}
using PujcovadloServer.Filters;
using PujcovadloServer.Lib;
using PujcovadloServer.Models;

namespace PujcovadloServer.Repositories.Interfaces;

public interface IItemRepository : ICrudRepository<Item>
{
    public Task<PaginatedList<Item>> GetAll(ItemFilter filter);
}
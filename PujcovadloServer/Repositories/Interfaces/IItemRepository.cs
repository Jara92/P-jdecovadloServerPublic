using PujcovadloServer.Filters;
using PujcovadloServer.Lib;
using PujcovadloServer.Models;

namespace PujcovadloServer.Repositories.Interfaces;

public interface IItemRepository : ICrudRepository<Item, ItemFilter>
{
   
}
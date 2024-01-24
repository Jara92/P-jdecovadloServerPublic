using PujcovadloServer.Business.Entities;
using PujcovadloServer.Business.Filters;

namespace PujcovadloServer.Business.Interfaces;

public interface IItemRepository : ICrudRepository<Item, ItemFilter>
{
   
}
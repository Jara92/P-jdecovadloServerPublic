using PujcovadloServer.Business.Entities;
using PujcovadloServer.Business.Filters;
using PujcovadloServer.Business.Objects;

namespace PujcovadloServer.Business.Interfaces;

public interface IItemTagRepository : ICrudRepository<ItemTag, ItemTagFilter>
{
    public Task<ItemTag?> GetByName(string name);

    Task<IEnumerable<ItemTag>> GetByIds(ICollection<int> requestTags);

    Task<IList<ItemTagOption>> GetAllAsOptions();
}
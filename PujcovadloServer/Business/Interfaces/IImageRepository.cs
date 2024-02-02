using PujcovadloServer.Business.Entities;
using PujcovadloServer.Business.Filters;

namespace PujcovadloServer.Business.Interfaces;

public interface IImageRepository : ICrudRepository<Image, BaseFilter>
{
    public Task<IList<Image>> GetByIds(IEnumerable<int> ids);
}
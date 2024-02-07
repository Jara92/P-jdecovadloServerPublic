using PujcovadloServer.Business.Entities;
using PujcovadloServer.Business.Filters;
using PujcovadloServer.Business.Interfaces;

namespace PujcovadloServer.Business.Services;

public class ImageService(IImageRepository repository)
    : ACrudService<Image, IImageRepository, BaseFilter>(repository)
{
    
    public async Task<IList<Image>> GetByIds(IEnumerable<int> ids)
    {
        return await _repository.GetByIds(ids);
    }

    public async Task<Image?> GetByPath(string name)
    {
        return await _repository.GetByPath(name);
    }
}
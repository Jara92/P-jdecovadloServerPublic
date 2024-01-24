using PujcovadloServer.Business.Filters;
using PujcovadloServer.Lib;

namespace PujcovadloServer.Business.Services.Interfaces;

public interface ICrudService<T, G> where T : class where G : BaseFilter
{
    public Task<PaginatedList<T>> GetAll(G filter);

    public Task<T?> Get(int id, bool track = true);

    public Task Create(T entity);

    public Task Update(T entity);

    public Task Delete(int id);
    
    public Task Delete(T entity);
}
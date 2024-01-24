using PujcovadloServer.Filters;
using PujcovadloServer.Lib;

namespace PujcovadloServer.Repositories.Interfaces;

public interface ICrudRepository<T>
{
    public Task<PaginatedList<T>> GetAll(BaseFilter filter);

    public Task<T?> Get(int id);
    
    public Task<T?> GetUntracked(int id);

    public Task Create(T entity);

    public Task Update(T entity);
    
    public Task Delete(T entity);
}
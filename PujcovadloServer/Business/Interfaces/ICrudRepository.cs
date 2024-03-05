using PujcovadloServer.Lib;

namespace PujcovadloServer.Business.Interfaces;

public interface ICrudRepository<T, G> : IPaginableRepository<T>
{
    public Task<PaginatedList<T>> GetAll(G filter);

    public Task<T?> Get(int id);

    public Task<T?> GetUntracked(int id);

    public Task Create(T entity);

    public Task Update(T entity);

    public Task Delete(T entity);
}
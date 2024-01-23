namespace PujcovadloServer.Services.Interfaces;

public interface ICrudService<T>
{
    public Task<List<T>> GetAll();

    public Task<T?> Get(int id, bool track = true);

    public Task Create(T entity);

    public Task Update(T entity);

    public Task Delete(int id);
    
    public Task Delete(T entity);
}
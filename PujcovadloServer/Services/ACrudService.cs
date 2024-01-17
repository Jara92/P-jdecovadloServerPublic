using System.ComponentModel;
using PujcovadloServer.Repositories;
using PujcovadloServer.Repositories.Interfaces;
using PujcovadloServer.Services.Interfaces;

namespace PujcovadloServer.Services;

/// <summary>
/// Default implementation of <see cref="ICrudService{T}"/>.
/// Implements all CRUD base methods.
/// </summary>
/// <typeparam name="T">Managed entity</typeparam>
public abstract class ACrudService<T> : ICrudService<T> where T : class
{
    protected readonly ICrudRepository<T> _repository;

    protected ACrudService(ICrudRepository<T> repository)
    {
        _repository = repository;
    }

    public virtual async Task<List<T>> GetAll()
    {
        return await _repository.GetAll();
    }

    public virtual async Task<T?> Get(int id)
    {
        return await _repository.Get(id);
    }

    public virtual async Task Create(T entity)
    {
        await _repository.Create(entity);
    }

    public virtual async Task Update(T entity)
    {
        await _repository.Update(entity);
    }

    public virtual async Task Delete(T entity)
    {
        await _repository.Delete(entity);
    }
    
    /*public virtual void FillRequest(T entity, object request)
    {
        var editableProperties = request.GetType()
            .GetProperties()
            .Where(property => !Attribute.IsDefined(property, typeof(ReadOnlyAttribute)))
            .ToList();

        foreach (var property in editableProperties)
        {
            var newValue = property.GetValue(request);
            property.SetValue(entity, newValue);
        }
    }*/
}
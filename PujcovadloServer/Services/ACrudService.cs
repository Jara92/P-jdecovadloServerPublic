using System.ComponentModel;
using PujcovadloServer.Exceptions;
using PujcovadloServer.Filters;
using PujcovadloServer.Lib;
using PujcovadloServer.Models;
using PujcovadloServer.Repositories;
using PujcovadloServer.Repositories.Interfaces;
using PujcovadloServer.Services.Interfaces;

namespace PujcovadloServer.Services;

/// <summary>
/// Default implementation of <see cref="ICrudService{T}"/>.
/// Implements all CRUD base methods.
/// </summary>
/// <typeparam name="T">Managed entity</typeparam>
public abstract class ACrudService<T, G> : ICrudService<T, G> where T : BaseEntity where G : BaseFilter
{
    protected readonly ICrudRepository<T, G> _repository;

    protected ACrudService(ICrudRepository<T, G> repository)
    {
        _repository = repository;
    }

    public virtual async Task<PaginatedList<T>> GetAll(G filter)
    {
        return await _repository.GetAll(filter);
    }

    public virtual async Task<T?> Get(int id, bool track = true)
    {
        if(track)
            return await _repository.Get(id);
        else
            return await _repository.GetUntracked(id);
    }

    public virtual async Task Create(T entity)
    {
        await _repository.Create(entity);
    }

    public virtual async Task Update(T entity)
    {
        await _repository.Update(entity);
    }
    
    public virtual async Task Delete(int id)
    {
        var entity = await _repository.Get(id);
        
        if(entity is null)
            throw new EntityNotFoundException($"Entity with id {id} not found.");

        await Delete(entity);
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
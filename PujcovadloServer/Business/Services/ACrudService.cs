using System.Collections;
using PujcovadloServer.Business.Entities;
using PujcovadloServer.Business.Exceptions;
using PujcovadloServer.Business.Filters;
using PujcovadloServer.Business.Interfaces;
using PujcovadloServer.Business.Services.Interfaces;
using PujcovadloServer.Lib;
using Syncfusion.EJ2.Base;

namespace PujcovadloServer.Business.Services;

/// <summary>
/// Default implementation of <see cref="ICrudService{T}"/>.
/// Implements all CRUD base methods.
/// </summary>
/// <typeparam name="T">Managed entity</typeparam>
/// <typeparam name="R">Repository class</typeparam>
/// <typeparam name="G">Filter class</typeparam>
public abstract class ACrudService<T, R, G> : ICrudService<T, R, G> where T : BaseEntity
    where R : ICrudRepository<T, G>
    where G : BaseFilter
{
    protected readonly R _repository;

    protected ACrudService(R repository)
    {
        _repository = repository;
    }

    public virtual async Task<PaginatedList<T>> GetAll(G filter)
    {
        return await _repository.GetAll(filter);
    }

    public Task<List<T>> GetAll(DataManagerRequest dm)
    {
        return _repository.GetAll(dm);
    }

    public Task<IEnumerable> GetAggregations(DataManagerRequest dm)
    {
        return _repository.GetAggregations(dm);
    }

    public Task<int> GetCount(DataManagerRequest dm)
    {
        return _repository.GetCount(dm);
    }

    public virtual async Task<T?> Get(int id, bool track = true)
    {
        if (track)
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

        if (entity is null)
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
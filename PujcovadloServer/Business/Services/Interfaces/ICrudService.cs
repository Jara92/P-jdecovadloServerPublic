using System.Collections;
using PujcovadloServer.Business.Entities;
using PujcovadloServer.Business.Filters;
using PujcovadloServer.Business.Interfaces;
using PujcovadloServer.Lib;
using Syncfusion.EJ2.Base;

namespace PujcovadloServer.Business.Services.Interfaces;

/// <summary>
/// 
/// </summary>
/// <typeparam name="T">Managed entity</typeparam>
/// <typeparam name="R">Repository class</typeparam>
/// <typeparam name="G">Filter class</typeparam>
public interface ICrudService<T, R, G> where T : BaseEntity where R : ICrudRepository<T, G> where G : BaseFilter
{
    public Task<PaginatedList<T>> GetAll(G filter);

    public Task<List<T>> GetAll(DataManagerRequest dm);

    public Task<IEnumerable> GetAggregations(DataManagerRequest dm);

    public Task<int> GetCount(DataManagerRequest dm);

    public Task<T?> Get(int id, bool track = true);

    public Task Create(T entity);

    public Task Update(T entity);

    public Task Delete(int id);

    public Task Delete(T entity);
}
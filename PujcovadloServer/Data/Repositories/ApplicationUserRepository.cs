using System.Collections;
using Microsoft.EntityFrameworkCore;
using PujcovadloServer.Authentication;
using PujcovadloServer.Business.Filters;
using PujcovadloServer.Business.Interfaces;
using PujcovadloServer.Business.Objects;
using PujcovadloServer.Lib;
using Syncfusion.EJ2.Base;

namespace PujcovadloServer.Data.Repositories;

public class ApplicationUserRepository : IApplicationUserRepository
{
    private readonly PujcovadloServerContext _context;
    private readonly DbSet<ApplicationUser> _dbSet;

    /// <summary>
    /// Defines maximum returned records count.
    /// </summary>
    public int MaximumReturnedRecords { get; set; } = 200;

    public ApplicationUserRepository(PujcovadloServerContext context)
    {
        _context = context;
        _dbSet = context.Users;
    }

    public async Task<PaginatedList<ApplicationUser>> GetAll(ApplicationUserFilter filter)
    {
        return await GetAll(_dbSet.AsQueryable(), filter);
    }

    protected async Task<PaginatedList<ApplicationUser>> GetAll(IQueryable<ApplicationUser> baseQuery,
        ApplicationUserFilter filter)
    {
        return await PaginatedList<ApplicationUser>.CreateAsync(baseQuery, filter.Page, filter.PageSize);
    }

    public Task<List<ApplicationUser>> GetAll(DataManagerRequest dm)
    {
        var query = GetFilteredQuery(dm);
        var operations = new DataOperations();

        if (dm.Skip != 0)
        {
            query = operations.PerformSkip(query, dm.Skip); //Paging
        }

        // MAke sure we have a limit
        if (dm.Take == 0) dm.Take = MaximumReturnedRecords;

        query = operations.PerformTake(query, dm.Take);


        return query.ToListAsync();
    }

    public Task<IEnumerable> GetAggregations(DataManagerRequest dm)
    {
        var query = GetFilteredQuery(dm);

        List<string> str = new List<string>();
        if (dm.Aggregates != null)
        {
            for (var i = 0; i < dm.Aggregates.Count; i++)
                str.Add(dm.Aggregates[i].Field);
        }

        var operations = new DataOperations();

        IEnumerable aggregate = operations.PerformSelect(query, str);

        return Task.FromResult(aggregate);
    }

    public Task<int> GetCount(DataManagerRequest dm)
    {
        return GetFilteredQuery(dm).CountAsync();
    }

    protected virtual IQueryable<ApplicationUser> GetFilteredQuery(DataManagerRequest dm)
    {
        var query = _dbSet.AsNoTracking().AsQueryable();

        var operations = new DataOperations();

        if (dm.Search != null && dm.Search.Count > 0)
        {
            query = operations.PerformSearching(query, dm.Search); //Search
        }

        if (dm.Sorted != null && dm.Sorted.Count > 0) //Sorting
        {
            query = operations.PerformSorting(query, dm.Sorted);
        }
        else
        {
            // Sort by registration date by default
            query = query.OrderByDescending(x => x.CreatedAt);
        }

        if (dm.Where != null && dm.Where.Count > 0) //Filtering
        {
            query = operations.PerformFiltering(query, dm.Where, dm.Where[0].Operator);
        }

        return query;
    }

    public async Task<ApplicationUser?> Get(string id)
    {
        return await _dbSet.FindAsync(id);
    }

    public async Task Create(ApplicationUser entity)
    {
        await _dbSet.AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public async Task Update(ApplicationUser entity)
    {
        _context.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task Delete(ApplicationUser entity)
    {
        _context.Remove(entity);
        await _context.SaveChangesAsync();
    }

    public async Task<IList<ApplicationUserOption>> GetAllAsOptions(ApplicationUserFilter filter)
    {
        return await _dbSet
            .Select(x => new ApplicationUserOption
            {
                Id = x.Id,
                FirstName = x.FirstName,
                LastName = x.LastName,
            })
            .ToListAsync();
    }
}
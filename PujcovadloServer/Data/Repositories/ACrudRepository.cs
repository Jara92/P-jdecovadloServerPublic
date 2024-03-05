using System.Collections;
using Microsoft.EntityFrameworkCore;
using PujcovadloServer.Business.Entities;
using PujcovadloServer.Business.Filters;
using PujcovadloServer.Business.Interfaces;
using PujcovadloServer.Lib;
using Syncfusion.EJ2.Base;

namespace PujcovadloServer.Data.Repositories
{
    public abstract class ACrudRepository<T, G> : ICrudRepository<T, G> where T : BaseEntity where G : BaseFilter
    {
        protected readonly PujcovadloServerContext _context;
        protected readonly DbSet<T> _dbSet;

        protected ACrudRepository(PujcovadloServerContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public virtual async Task<PaginatedList<T>> GetAll(G filter)
        {
            return await GetAll(_dbSet.AsQueryable(), filter);
        }

        public Task<List<T>> GetAll(DataManagerRequest dm)
        {
            var query = GetFilteredQuery(dm);
            var operations = new DataOperations();

            if (dm.Skip != 0)
            {
                query = operations.PerformSkip(query, dm.Skip); //Paging
            }

            if (dm.Take != 0)
            {
                query = operations.PerformTake(query, dm.Take);
            }

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

        protected virtual IQueryable<T> GetFilteredQuery(DataManagerRequest dm)
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

            if (dm.Where != null && dm.Where.Count > 0) //Filtering
            {
                query = operations.PerformFiltering(query, dm.Where, dm.Where[0].Operator);
            }

            return query;
        }

        protected virtual async Task<PaginatedList<T>> GetAll(IQueryable<T> baseQuery, G filter)
        {
            return await PaginatedList<T>.CreateAsync(baseQuery, filter.Page, filter.PageSize);
        }

        public virtual async Task<T?> Get(int id)
        {
            return await _dbSet.FindAsync(id);
        }

        public virtual async Task<T?> GetUntracked(int id)
        {
            return await _dbSet.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id);
        }

        public virtual async Task Create(T entity)
        {
            await _dbSet.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public virtual async Task Update(T entity)
        {
            _context.Update(entity);
            await _context.SaveChangesAsync();
        }

        public virtual async Task Delete(T entity)
        {
            _context.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}
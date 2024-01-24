using Microsoft.EntityFrameworkCore;
using PujcovadloServer.Business.Entities;
using PujcovadloServer.Business.Filters;
using PujcovadloServer.Business.Interfaces;
using PujcovadloServer.Lib;

namespace PujcovadloServer.Data.Repositories
{
    public abstract class ACrudRepository<T, G> : ICrudRepository<T, G> where T : BaseEntity where G: BaseFilter
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
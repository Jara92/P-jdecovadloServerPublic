using Microsoft.EntityFrameworkCore;
using PujcovadloServer.Authentication;
using PujcovadloServer.Business.Filters;
using PujcovadloServer.Business.Interfaces;
using PujcovadloServer.Business.Objects;
using PujcovadloServer.Lib;

namespace PujcovadloServer.Data.Repositories;

public class ApplicationUserRepository : IApplicationUserRepository
{
    private readonly PujcovadloServerContext _context;
    private readonly DbSet<ApplicationUser> _dbSet;

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
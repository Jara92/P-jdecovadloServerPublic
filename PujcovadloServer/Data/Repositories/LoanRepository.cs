using Microsoft.EntityFrameworkCore;
using PujcovadloServer.Business.Entities;
using PujcovadloServer.Business.Enums;
using PujcovadloServer.Business.Filters;
using PujcovadloServer.Business.Interfaces;
using PujcovadloServer.Lib;

namespace PujcovadloServer.Data.Repositories;

public class LoanRepository : ACrudRepository<Loan, LoanFilter>, ILoanRepository
{
    private readonly PujcovadloServerContext _context;
    private readonly DbSet<Loan> _dbSet;

    // TODO: Move this to a service
    private readonly List<LoanStatus> _finalStatuses = new()
        { LoanStatus.Denied, LoanStatus.Returned, LoanStatus.Cancelled };

    public LoanRepository(PujcovadloServerContext context) : base(context)
    {
        _context = context;
        _dbSet = context.Loan;
    }

    public override Task<PaginatedList<Loan>> GetAll(LoanFilter filter)
    {
        var query = _dbSet.AsQueryable();

        // Filter by tenant
        if (filter.TenantId != null)
            query = query.Where(l => l.TenantId == filter.TenantId);

        // Filter by owner
        if (filter.OwnerId != null)
            query = query.Where(l => l.Item.OwnerId == filter.OwnerId);

        return base.GetAll(query, filter);
    }

    public Task<List<Loan>> GetRunningLoansByItem(Item item)
    {
        return _dbSet.Where(l => l.Item.Id == item.Id)
            .Where(l => !_finalStatuses.Contains(l.Status))
            .ToListAsync();
    }

    public Task<int> GetRunningLoansCountByItem(Item item)
    {
        return _dbSet.Where(l => l.Item.Id == item.Id)
            .Where(l => !_finalStatuses.Contains(l.Status))
            .CountAsync();
    }

    public Task<PaginatedList<Loan>> GetAllByUserId(string userId, LoanFilter filter)
    {
        var query = _dbSet.AsQueryable();

        // Filter by user no matter if he is tenant or owner
        query = query.Where(l => l.TenantId == userId || l.Item.OwnerId == userId);

        return base.GetAll(query, filter);
    }

    public Task<int> GetBorrovedItemsCountByUser(string userId)
    {
        return _dbSet.Where(l => l.TenantId == userId)
            .Where(l => _finalStatuses.Contains(l.Status))
            .CountAsync();
    }

    public Task<int> GetLentItemsCountByUser(string userId)
    {
        return _dbSet.Where(l => l.Item.OwnerId == userId)
            .Where(l => _finalStatuses.Contains(l.Status))
            .CountAsync();
    }
}
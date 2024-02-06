using Microsoft.EntityFrameworkCore;
using PujcovadloServer.Business.Entities;
using PujcovadloServer.Business.Enums;
using PujcovadloServer.Business.Exceptions;
using PujcovadloServer.Business.Filters;
using PujcovadloServer.Business.Interfaces;
using PujcovadloServer.Lib;

namespace PujcovadloServer.Data.Repositories;

public class LoanRepository : ACrudRepository<Loan, LoanFilter>, ILoanRepository
{
    private readonly PujcovadloServerContext _context;
    private readonly DbSet<Loan> _dbSet;
    
    private readonly List<LoanStatus> _finalStatuses = new() {LoanStatus.Denied, LoanStatus.Returned, LoanStatus.Cancelled};

    public LoanRepository(PujcovadloServerContext context) : base(context)
    {
        _context = context;
        _dbSet = context.Loan;
    }

    public override Task<PaginatedList<Loan>> GetAll(LoanFilter filter)
    {
        var query = _dbSet.AsQueryable();
        
        // Filter by tenant
        if(filter.TenantId != null)
            query = query.Where(l => l.Tenant.Id == filter.TenantId);

        // Filter by owner
        if (filter.OwnerId != null)
            query = query.Where(l => l.Item.Owner.Id == filter.OwnerId);
        
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
}
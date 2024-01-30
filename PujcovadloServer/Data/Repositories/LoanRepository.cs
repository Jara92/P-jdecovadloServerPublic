using Microsoft.EntityFrameworkCore;
using PujcovadloServer.Business.Entities;
using PujcovadloServer.Business.Exceptions;
using PujcovadloServer.Business.Filters;
using PujcovadloServer.Business.Interfaces;
using PujcovadloServer.Lib;

namespace PujcovadloServer.Data.Repositories;

public class LoanRepository : ACrudRepository<Loan, LoanFilter>, ILoanRepository
{
    private readonly PujcovadloServerContext _context;
    private readonly DbSet<Loan> _dbSet;

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
}
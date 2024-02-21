using Microsoft.EntityFrameworkCore;
using PujcovadloServer.Authentication;
using PujcovadloServer.Business.Entities;
using PujcovadloServer.Business.Filters;
using PujcovadloServer.Business.Interfaces;
using PujcovadloServer.Lib;

namespace PujcovadloServer.Data.Repositories;

public class ReviewRepository : ACrudRepository<Review, ReviewFilter>, IReviewRepository
{
    private readonly PujcovadloServerContext _context;
    private readonly DbSet<Review> _dbSet;

    public ReviewRepository(PujcovadloServerContext context) : base(context)
    {
        _context = context;
        _dbSet = context.Review;
    }

    public Task<PaginatedList<Review>> GetAllByTargetUser(ApplicationUser user, ReviewFilter filter)
    {
        var query = _dbSet.AsQueryable();

        // Dont return user's own reviews
        query = query.Where(r => r.Author.Id != user.Id);

        // Show only reviews where the user is the target
        // TODO: wtf
        // Přidta OwnerId
        query = query.Where(r => r.Loan.Item.Owner.Id == user.Id || r.Loan.Tenant.Id == user.Id);

        return base.GetAll(query, filter);
    }

    /// <inheritdoc cref="IReviewRepository"/>
    public Task<Review?> FindByLoanAndAuthor(Loan reviewLoan, ApplicationUser reviewAuthor)
    {
        return _dbSet.FirstOrDefaultAsync(r => r.Loan.Id == reviewLoan.Id && r.Author.Id == reviewAuthor.Id);
    }
}
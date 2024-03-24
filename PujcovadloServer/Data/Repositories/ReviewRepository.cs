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

    /// <summary>
    /// Returns a query which filters reviews by the target user and the filter.
    /// </summary>
    /// <param name="userId">Target userId</param>
    /// <param name="filter">Filter object</param>
    /// <returns>IQueryable query</returns>
    private IQueryable<Review> GetQueryAllByTargetUser(string userId, ReviewFilter filter)
    {
        var query = _dbSet.AsQueryable();

        // Dont return user's own reviews
        query = query.Where(r => r.Author.Id != userId);

        // Show only reviews where the user is the target
        query = query.Where(r => r.Loan.Item.OwnerId == userId || r.Loan.TenantId == userId);

        return query;
    }

    public Task<PaginatedList<Review>> GetAllByTargetUser(ApplicationUser user, ReviewFilter filter)
    {
        var query = GetQueryAllByTargetUser(user.Id, filter);

        return base.GetAll(query, filter);
    }

    /// <inheritdoc cref="IReviewRepository"/>
    public Task<Review?> FindByLoanAndAuthor(Loan reviewLoan, ApplicationUser reviewAuthor)
    {
        return _dbSet.FirstOrDefaultAsync(r => r.Loan.Id == reviewLoan.Id && r.AuthorId == reviewAuthor.Id);
    }

    /// <inheritdoc cref="IReviewRepository"/>
    public Task<float?> GetAverageRatingForUser(string userId)
    {
        // Filter reviews by the target user
        var query = GetQueryAllByTargetUser(userId, new ReviewFilter());

        // Return the average rating
        return query.AverageAsync(r => (float?)r.Rating);
    }

    public Task<int> GetTotalReviewsCountForUser(string userId)
    {
        // Filter reviews by the target user
        var query = GetQueryAllByTargetUser(userId, new ReviewFilter());

        // Return the count of reviews
        return query.CountAsync();
    }
}
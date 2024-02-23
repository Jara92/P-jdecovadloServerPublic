using PujcovadloServer.Authentication;
using PujcovadloServer.Business.Entities;
using PujcovadloServer.Business.Filters;
using PujcovadloServer.Lib;

namespace PujcovadloServer.Business.Interfaces;

public interface IReviewRepository : ICrudRepository<Review, ReviewFilter>
{
    Task<PaginatedList<Review>> GetAllByTargetUser(ApplicationUser user, ReviewFilter filter);

    /// <summary>
    /// Returns review by loan and author.
    /// </summary>
    /// <param name="reviewLoan"></param>
    /// <param name="reviewAuthor"></param>
    /// <returns>Returns the review or null if the review was not found</returns>
    Task<Review?> FindByLoanAndAuthor(Loan reviewLoan, ApplicationUser reviewAuthor);

    Task<float?> GetAverageRatingForUser(string userId);
}
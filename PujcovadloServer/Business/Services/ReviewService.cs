using PujcovadloServer.Authentication;
using PujcovadloServer.Business.Entities;
using PujcovadloServer.Business.Filters;
using PujcovadloServer.Business.Interfaces;
using PujcovadloServer.Lib;

namespace PujcovadloServer.Business.Services;

public class ReviewService(IReviewRepository repository)
    : ACrudService<Review, IReviewRepository, ReviewFilter>(repository)
{
    public override async Task<PaginatedList<Review>> GetAll(ReviewFilter filter)
    {
        return await _repository.GetAll(filter);
    }

    public Task<PaginatedList<Review>> GetAllByTargetUser(ApplicationUser user, ReviewFilter filter)
    {
        return _repository.GetAllByTargetUser(user, filter);
    }

    public virtual Task<Review?> FindByLoanAndAuthor(Loan reviewLoan, ApplicationUser reviewAuthor)
    {
        return _repository.FindByLoanAndAuthor(reviewLoan, reviewAuthor);
    }

    public override async Task Update(Review item)
    {
        item.UpdatedAt = DateTime.Now;
        await base.Update(item);
    }

    public override Task Create(Review entity)
    {
        entity.CreatedAt = DateTime.Now;
        return base.Create(entity);
    }

    public override Task Delete(Review entity)
    {
        entity.DeletedAt = DateTime.Now;

        return base.Delete(entity);
    }
}
using AutoMapper;
using PujcovadloServer.Areas.Admin.Requests;
using PujcovadloServer.Business.Entities;
using PujcovadloServer.Business.Exceptions;
using PujcovadloServer.Business.Interfaces;
using PujcovadloServer.Business.Services;

namespace PujcovadloServer.Areas.Admin.Business.Facades;

public class ReviewFacade
{
    private readonly ReviewService _reviewService;
    private readonly IFileStorage _storage;

    public ReviewFacade(ReviewService reviewService, IMapper mapper)
    {
        _reviewService = reviewService;
        ;
    }

    public async Task<Review> Get(int id)
    {
        var item = await _reviewService.Get(id);
        if (item == null) throw new EntityNotFoundException();

        return item;
    }


    public async Task Update(Review review, ReviewRequest request)
    {
        review.Comment = request.Comment;
        review.Rating = request.Rating;

        review.AuthorId = request.AuthorId;
        review.LoanId = request.LoanId;

        review.CreatedAt = request.CreatedAt;
        review.UpdatedAt = request.UpdatedAt; // todo: not actually used because updatedAt is overwritten in service
        review.DeletedAt = request.DeletedAt;

        await _reviewService.Update(review);
    }

    public Task Delete(Review item)
    {
        return _reviewService.Delete(item);
    }
}
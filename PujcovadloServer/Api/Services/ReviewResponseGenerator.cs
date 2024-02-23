using AutoMapper;
using PujcovadloServer.Api.Controllers;
using PujcovadloServer.AuthorizationHandlers;
using PujcovadloServer.Business.Entities;
using PujcovadloServer.Business.Filters;
using PujcovadloServer.Lib;
using PujcovadloServer.Responses;

namespace PujcovadloServer.Api.Services;

/// <summary>
/// This class generates responses for Review entity.
/// Generates pagination and HATEOAS links for the responses.
/// </summary>
public class ReviewResponseGenerator : ABaseResponseGenerator
{
    private readonly IMapper _mapper;

    public ReviewResponseGenerator(IMapper mapper, LinkGenerator urlHelper,
        IHttpContextAccessor httpContextAccessor,
        AuthorizationService authorizationService) :
        base(httpContextAccessor, urlHelper, authorizationService)
    {
        _mapper = mapper;
    }

    /// <summary>
    /// generates ReviewResponse for single review.
    /// </summary>
    /// <param name="review">Review to be converted to a response</param>
    /// <returns>ReviewResponse which represents the Review.</returns>
    private async Task<ReviewResponse> GenerateSingleReviewResponse(Review review)
    {
        var response = _mapper.Map<ReviewResponse>(review);

        // Add link to detail
        response._links.Add(new LinkResponse(GetLink(review), "SELF", "GET"));

        AddCommonLinks(response, review);

        return response;
    }

    /// <summary>
    /// generates links which are common for list and detail responses.
    /// </summary>
    /// <param name="response">Response to be advanced.</param>
    /// <param name="review">Review which is the response about.</param>
    private void AddCommonLinks(ReviewResponse response, Review review)
    {
        // TODO: uncomment when user controller is ready
        /*
         response._links.Add(new LinkResponse(
            _urlHelper.GetUriByAction(HttpContext, nameof(Get), "User", values: new { review.Item.Owner.Id }),
            "OWNER", "GET"));

        response._links.Add(new LinkResponse(
           _urlHelper.GetUriByAction(HttpContext, nameof(Get), "User", values: new { review.Tenant.Id }),
           "TENANT", "GET"));
        */

        // Review item
        response._links.Add(new LinkResponse(
            _urlHelper.GetUriByAction(_httpContext, nameof(ItemController.Get), "Item",
                values: new { review.Loan.Item.Id }), "ITEM", "GET"));

        // TODO: Add update link
    }

    /// <summary>
    /// Returns ResponseList with all reviews as responses.
    /// </summary>
    /// <param name="reviews">Categories to be converted to a response.</param>
    /// <param name="filter">Filter used for retrieving the reviews.</param>
    /// <param name="action">Action to used for retrieving the reviews.</param>
    /// <param name="controller">Controller used for retrieving the reviews.</param>
    /// <returns>ReviewResponse</returns>
    public async Task<ResponseList<ReviewResponse>> GenerateResponseList(PaginatedList<Review> reviews,
        ReviewFilter filter,
        string action, string controller)
    {
        // Create empty list
        var responseItems = new List<ReviewResponse>();

        foreach (var loan in reviews)
        {
            responseItems.Add(await GenerateSingleReviewResponse(loan));
        }

        // Init links with pagination links
        var links = GeneratePaginationLinks(reviews, filter, action, controller);

        // Todo: links

        // return response list
        return new ResponseList<ReviewResponse>
        {
            _data = responseItems,
            _links = links
        };
    }

    /// <summary>
    /// generates itemDetailResponse with detailed information about the review.
    /// </summary>
    /// <param name="review">Review to be converted to a response.</param>
    /// <returns>ReviewResponse which represents the review</returns>
    public async Task<ReviewResponse> GenerateReviewDetailResponse(Review review)
    {
        var response = _mapper.Map<ReviewResponse>(review);

        AddCommonLinks(response, review);

        // Add link to all reviews where the user participates
        /*response._links.Add(new LinkResponse(
            _urlHelper.GetUriByAction(_httpContext, nameof(ReviewController.GetReviews), "CreateReview"),
            "LIST", "GET"));*/

        return response;
    }

    public string? GetLink(Review loan)
    {
        return _urlHelper.GetUriByAction(_httpContext, nameof(ReviewController.GetReview), "Review", values: loan.Id);
    }
}
using Microsoft.AspNetCore.Mvc;
using PujcovadloServer.Areas.Api.Filters;
using PujcovadloServer.Areas.Api.Services;
using PujcovadloServer.AuthorizationHandlers;
using PujcovadloServer.AuthorizationHandlers.Review;
using PujcovadloServer.Business.Entities;
using PujcovadloServer.Business.Facades;
using PujcovadloServer.Responses;

namespace PujcovadloServer.Areas.Api.Controllers;

[Area("Api")]
[ApiController]
[Route("api/reviews")]
[ServiceFilter(typeof(ExceptionFilter))]
public class ReviewController : ACrudController<Loan>
{
    private readonly ReviewFacade _reviewFacade;
    private readonly ReviewResponseGenerator _responseGenerator;

    public ReviewController(ReviewFacade reviewFacade, ReviewResponseGenerator responseGenerator,
        AuthorizationService authorizationService, LinkGenerator urlHelper) : base(authorizationService, urlHelper)
    {
        _reviewFacade = reviewFacade;
        _responseGenerator = responseGenerator;
    }

    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReviewResponse>> GetReview(int id)
    {
        var review = await _reviewFacade.GetReview(id);

        await _authorizationService.CheckPermissions(review, ReviewOperations.Read);

        // Generate response
        var response = await _responseGenerator.GenerateReviewDetailResponse(review);

        return Ok(response);
    }
}
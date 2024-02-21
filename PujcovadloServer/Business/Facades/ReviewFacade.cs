using AutoMapper;
using PujcovadloServer.Authentication;
using PujcovadloServer.Business.Entities;
using PujcovadloServer.Business.Exceptions;
using PujcovadloServer.Business.Filters;
using PujcovadloServer.Business.Services;
using PujcovadloServer.Business.Services.Interfaces;
using PujcovadloServer.Lib;
using PujcovadloServer.Requests;

namespace PujcovadloServer.Business.Facades;

public class ReviewFacade
{
    private readonly ReviewService _reviewService;
    private readonly LoanService _loanService;
    private readonly IAuthenticateService _authenticateService;
    private readonly IMapper _mapper;
    private readonly PujcovadloServerConfiguration _configuration;

    public ReviewFacade(ReviewService reviewService, LoanService loanService,
        IAuthenticateService authenticateService, IMapper mapper,
        PujcovadloServerConfiguration configuration)
    {
        _reviewService = reviewService;
        _loanService = loanService;
        _authenticateService = authenticateService;
        _mapper = mapper;
        _configuration = configuration;
    }

    /// <summary>
    /// Returns all reviews by given filter.
    /// </summary>
    /// <param name="user">User whose reviews should be returned.</param>
    /// <param name="filter">Filter data</param>
    /// <returns>Paginated and filtered reviews</returns>
    public Task<PaginatedList<Review>> GetAll(ApplicationUser user, ReviewFilter filter)
    {
        return _reviewService.GetAllByTargetUser(user, filter);
    }

    /// <summary>
    /// Fill request data to the review.
    /// </summary>
    /// <param name="review">The review.</param>
    /// <param name="request">Review request data.</param>
    private Task FillReviewRequest(Review review, ReviewRequest request)
    {
        // Map request to review
        review.Comment = request.Comment;
        review.Rating = request.Rating;

        return Task.CompletedTask;
    }

    /// <summary>
    /// Can the review be created for the loan by the user?
    /// </summary>
    /// <param name="loan">Loan for which the review should be created.</param>
    /// <param name="user">User who wants to create a review. Current user used if null.</param>
    /// <returns>CanCreate = True if the user can create a review.</returns>
    public async Task<(bool CanCreate, string Reason)> CanCreateReview(Loan loan, ApplicationUser? user = null)
    {
        // Get current user
        if (user == null)
        {
            user = await _authenticateService.GetCurrentUser();
        }

        // Find review by loan and author
        var foundReview = await _reviewService.FindByLoanAndAuthor(loan, user);

        // User cannot review the same loan twice
        if (foundReview != null)
        {
            return (false, "CreateReview already exists.");
        }

        // Get loan state
        var loanState = _loanService.GetState(loan);

        // Check if the loan can be reviewed
        if (loanState.CanCreateReview(loan) == false)
        {
            return (false, "Loan cannot be reviewed in current status.");
        }

        return (true, string.Empty);
    }

    /// <summary>
    /// Creates a new review using <see cref="ReviewRequest"/>
    /// </summary>
    /// <param name="loan">Loan to be reviewed</param>
    /// <param name="request">Request with review data.</param>
    public async Task<Review> CreateReview(Loan loan, ReviewRequest request)
    {
        var user = await _authenticateService.GetCurrentUser();

        // Verify that the review can be created
        var canCreateResult = await CanCreateReview(loan, user);
        if (canCreateResult.CanCreate == false)
        {
            throw new OperationNotAllowedException(canCreateResult.Reason);
        }

        // Fill request data
        var review = new Review();

        // Fill request data
        await FillReviewRequest(review, request);

        // set laon
        review.Loan = loan;

        // Set owner
        review.Author = user;

        // Create the review  
        await _reviewService.Create(review);

        return review;
    }

    /// <summary>
    /// Returns review with given id.
    /// </summary>
    /// <param name="id">Review's id</param>
    /// <returns>Review</returns>
    /// <exception cref="EntityNotFoundException">Thrown when review id is invalid.</exception>
    public async Task<Review> GetReview(int id)
    {
        var review = await _reviewService.Get(id);

        // Check if review exists
        if (review == null) throw new EntityNotFoundException($"Review with id {id} not found.");

        // Return review
        return review;
    }
}
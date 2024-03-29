using AutoMapper;
using PujcovadloServer.Areas.Api.Controllers;
using PujcovadloServer.AuthorizationHandlers;
using PujcovadloServer.AuthorizationHandlers.Loan;
using PujcovadloServer.Business.Entities;
using PujcovadloServer.Business.Facades;
using PujcovadloServer.Business.Filters;
using PujcovadloServer.Lib;
using PujcovadloServer.Responses;

namespace PujcovadloServer.Areas.Api.Services;

/// <summary>
/// This class generates responses for Loan entity.
/// Generates pagination and HATEOAS links for the responses.
/// </summary>
public class LoanResponseGenerator : ABaseResponseGenerator
{
    private readonly ImageResponseGenerator _imageResponseGenerator;
    private readonly IMapper _mapper;
    private readonly ReviewFacade _reviewFacade;
    private readonly ProfileFacade _profileFacade;

    public LoanResponseGenerator(ImageResponseGenerator imageResponseGenerator, ReviewFacade reviewFacade,
        ProfileFacade profileFacade, IMapper mapper, LinkGenerator urlHelper,
        IHttpContextAccessor httpContextAccessor, AuthorizationService authorizationService) :
        base(httpContextAccessor, urlHelper, authorizationService)
    {
        _imageResponseGenerator = imageResponseGenerator;
        _reviewFacade = reviewFacade;
        _profileFacade = profileFacade;
        _mapper = mapper;
    }

    /// <summary>
    /// generates LoanResponse for single loan.
    /// </summary>
    /// <param name="loan">Loan to be converted to a response</param>
    /// <returns>LoanResponse which represents the Loan.</returns>
    private async Task<LoanResponse> GenerateSingleLoanResponse(Loan loan)
    {
        var response = _mapper.Map<LoanResponse>(loan);

        // Add link to detail
        response._links.Add(new LinkResponse(GetLink(loan), "SELF", "GET"));

        await AddCommonLinks(response, loan);

        return response;
    }

    /// <summary>
    /// generates links which are common for list and detail responses.
    /// </summary>
    /// <param name="response">Response to be advanced.</param>
    /// <param name="loan">Loan which is the response about.</param>
    private async Task AddCommonLinks(LoanResponse response, Loan loan)
    {
        // Add image links
        if (loan.Item.MainImage != null)
        {
            response.ItemImage = await _imageResponseGenerator.GenerateImageDetailResponse(loan.Item.MainImage);
        }

        // Link to loan owner
        if (loan.Item.Owner.Profile != null)
        {
            response._links.Add(new LinkResponse(
                _urlHelper.GetUriByAction(_httpContext, nameof(UserController.GetUser), "User",
                    values: new { loan.Item.Owner.Id }),
                "OWNER", "GET"));
        }

        // Link to loan tenant
        if (loan.Tenant.Profile != null)
        {
            response._links.Add(new LinkResponse(
                _urlHelper.GetUriByAction(_httpContext, nameof(UserController.GetUser), "User",
                    values: new { loan.Tenant.Id }),
                "TENANT", "GET"));
        }

        // Loan item
        response._links.Add(new LinkResponse(
            _urlHelper.GetUriByAction(_httpContext, nameof(ItemController.Get), "Item",
                values: new { loan.Item.Id }), "ITEM", "GET"));

        // TODO: Add update link
    }

    /// <summary>
    /// Returns ResponseList with all loans as responses.
    /// </summary>
    /// <param name="loans">Categories to be converted to a response.</param>
    /// <param name="filter">Filter used for retrieving the loans.</param>
    /// <param name="action">Action to used for retrieving the loans.</param>
    /// <param name="controller">Controller used for retrieving the loans.</param>
    /// <returns>LoanResponse</returns>
    public async Task<ResponseList<LoanResponse>> GenerateResponseList(PaginatedList<Loan> loans,
        LoanFilter filter,
        string action, string controller)
    {
        // Create empty list
        var responseItems = new List<LoanResponse>();

        foreach (var loan in loans)
        {
            await AddUserAggregations(loan);
            responseItems.Add(await GenerateSingleLoanResponse(loan));
        }

        // Init links with pagination links
        var links = GeneratePaginationLinks(loans, filter, action, controller);

        // Todo: links

        // return response list
        return new ResponseList<LoanResponse>
        {
            _data = responseItems,
            _links = links
        };
    }

    /// <summary>
    /// generates itemDetailResponse with detailed information about the loan.
    /// </summary>
    /// <param name="loan">Loan to be converted to a response.</param>
    /// <returns>LoanResponse which represents the loan</returns>
    public async Task<LoanResponse> GenerateLoanDetailResponse(Loan loan)
    {
        await AddUserAggregations(loan);

        var response = _mapper.Map<LoanResponse>(loan);

        await AddCommonLinks(response, loan);

        // Add link to all loans where the user participates
        response._links.Add(new LinkResponse(
            _urlHelper.GetUriByAction(_httpContext, nameof(LoanController.GetLoans), "Loan"),
            "LIST", "GET"));

        // Link to review
        if (await _authorizationService.CanPerformOperation(loan, LoanOperations.CreateReview) // Check permissions
            && (await _reviewFacade.CanCreateReview(loan)).CanCreate) // Check if the review can be created
        {
            response._links.Add(new LinkResponse(
                _urlHelper.GetUriByAction(_httpContext, nameof(LoanController.CreateReview), "Loan",
                    values: new { loanId = loan.Id }), "CREATE_REVIEW", "POST"));
        }

        return response;
    }

    public string? GetLink(Loan loan)
    {
        return _urlHelper.GetUriByAction(_httpContext, nameof(LoanController.GetLoan), "Loan", values: loan.Id);
    }

    private async Task AddUserAggregations(Loan loan)
    {
        // Set aggregations for the owner
        var ownerProfile = loan.Item.Owner.Profile;
        if (ownerProfile != null)
        {
            ownerProfile.Aggregations = await _profileFacade.GetProfileAggregations(ownerProfile);
        }

        // Set aggregations for the tenant
        var tenantProfile = loan.Tenant.Profile;
        if (tenantProfile != null)
        {
            tenantProfile.Aggregations = await _profileFacade.GetProfileAggregations(tenantProfile);
        }
    }
}
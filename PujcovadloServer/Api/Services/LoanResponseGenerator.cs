using AutoMapper;
using PujcovadloServer.Api.Controllers;
using PujcovadloServer.AuthorizationHandlers;
using PujcovadloServer.AuthorizationHandlers.Item;
using PujcovadloServer.Business.Entities;
using PujcovadloServer.Business.Filters;
using PujcovadloServer.Lib;
using PujcovadloServer.Responses;

namespace PujcovadloServer.Api.Services;

/// <summary>
/// This class generates responses for Loan entity.
/// Generates pagination and HATEOAS links for the responses.
/// </summary>
public class LoanResponseGenerator : ABaseResponseGenerator
{
    private readonly IMapper _mapper;

    public LoanResponseGenerator(IMapper mapper, LinkGenerator urlHelper,
        IHttpContextAccessor httpContextAccessor,
        AuthorizationService authorizationService) :
        base(httpContextAccessor, urlHelper, authorizationService)
    {
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
        response.Links.Add(new LinkResponse(GetLink(loan), "SELF", "GET"));

        AddCommonLinks(response, loan);

        return response;
    }

    /// <summary>
    /// generates links which are common for list and detail responses.
    /// </summary>
    /// <param name="response">Response to be advanced.</param>
    /// <param name="loan">Loan which is the response about.</param>
    private void AddCommonLinks(LoanResponse response, Loan loan)
    {
        // TODO: uncomment when user controller is ready
        /*
         response.Links.Add(new LinkResponse(
            _urlHelper.GetUriByAction(HttpContext, nameof(Get), "User", values: new { loan.Item.Owner.Id }),
            "OWNER", "GET"));

        response.Links.Add(new LinkResponse(
           _urlHelper.GetUriByAction(HttpContext, nameof(Get), "User", values: new { loan.Tenant.Id }),
           "TENANT", "GET"));
        */

        // Loan item
        response.Links.Add(new LinkResponse(
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
            responseItems.Add(await GenerateSingleLoanResponse(loan));
        }

        // Init links with pagination links
        var links = GeneratePaginationLinks(loans, filter, action, controller);

        // Todo: links

        // return response list
        return new ResponseList<LoanResponse>
        {
            Data = responseItems,
            Links = links
        };
    }

    /// <summary>
    /// generates itemDetailResponse with detailed information about the loan.
    /// </summary>
    /// <param name="loan">Loan to be converted to a response.</param>
    /// <returns>LoanResponse which represents the loan</returns>
    public async Task<LoanResponse> GenerateLoanDetailResponse(Loan loan)
    {
        var response = _mapper.Map<LoanResponse>(loan);

        AddCommonLinks(response, loan);

        // Add links for the owner
        // TODO: remove this by merging the owner and tenant loans controllers
        if (await _authorizationService.CanPerformOperation(loan, LoanOperations.CreatePickupProtocol))
        {
            // Link to my owner loans
            response.Links.Add(new LinkResponse(
                _urlHelper.GetUriByAction(_httpContext, nameof(OwnerLoanController.GetLoans), "OwnerLoan"),
                "LIST", "GET"));
        }
        else
        {
            // Link to my tenant loans
            response.Links.Add(new LinkResponse(
                _urlHelper.GetUriByAction(_httpContext, nameof(TenantLoanController.GetLoans), "TenantLoan"),
                "LIST", "GET"));
        }

        return response;
    }

    public string? GetLink(Loan loan)
    {
        return _urlHelper.GetUriByAction(_httpContext, nameof(LoanController.GetLoan), "Loan", values: loan.Id);
    }
}
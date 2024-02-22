using Microsoft.AspNetCore.Mvc;
using PujcovadloServer.Api.Filters;
using PujcovadloServer.Api.Services;
using PujcovadloServer.AuthorizationHandlers;
using PujcovadloServer.AuthorizationHandlers.Item;
using PujcovadloServer.Business.Entities;
using PujcovadloServer.Business.Facades;
using PujcovadloServer.Business.Filters;
using PujcovadloServer.Requests;
using PujcovadloServer.Responses;

namespace PujcovadloServer.Api.Controllers;

[ApiController]
[Route("api/loans")]
[ServiceFilter(typeof(ExceptionFilter))]
public class LoanController : ACrudController<Loan>
{
    private readonly LoanFacade _loanFacade;
    private readonly TenantFacade _tenantFacade;
    private readonly OwnerFacade _ownerFacade;
    private readonly ItemFacade _itemFacade;
    private readonly ReviewFacade _reviewFacade;
    private readonly LoanResponseGenerator _responseGenerator;
    private readonly ReviewResponseGenerator _reviewResponseGenerator;

    public LoanController(LoanFacade loanFacade, TenantFacade tenantFacade, OwnerFacade ownerFacade,
        ItemFacade itemFacade, ReviewFacade reviewFacade,
        LoanResponseGenerator responseGenerator, ReviewResponseGenerator reviewResponseGenerator,
        AuthorizationService authorizationService, LinkGenerator urlHelper) : base(authorizationService, urlHelper)
    {
        _loanFacade = loanFacade;
        _tenantFacade = tenantFacade;
        _ownerFacade = ownerFacade;
        _itemFacade = itemFacade;
        _reviewFacade = reviewFacade;
        _responseGenerator = responseGenerator;
        _reviewResponseGenerator = reviewResponseGenerator;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<List<LoanResponse>>> GetLoans([FromQuery] LoanFilter filter)
    {
        var loans = await _loanFacade.GetLoans(filter);

        // generate response list
        var response = await _responseGenerator.GenerateResponseList(loans, filter, nameof(GetLoans), "Loan");

        return Ok(response);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ValidateIdFilter]
    public async Task<ActionResult<LoanResponse>> UpdateLoan(int id, [FromBody] LoanUpdateRequest request)
    {
        var loan = await _loanFacade.GetLoan(id);

        await _authorizationService.CheckPermissions(loan, LoanOperations.Update);

        await _loanFacade.UpdateLoan(loan, request);

        return Ok();
    }

    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LoanResponse>> GetLoan(int id)
    {
        var loan = await _loanFacade.GetLoan(id);

        await _authorizationService.CheckPermissions(loan, LoanOperations.Read);

        // Generate response
        var response = await _responseGenerator.GenerateLoanDetailResponse(loan);

        return Ok(response);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<LoanResponse>> CreateLoan([FromBody] LoanRequest request)
    {
        // Check that the users can create a new loan
        await _authorizationService.CheckPermissions(new Loan(), LoanOperations.Create);

        // Check if the item exists and that it can be used for a new loan
        var item = await _itemFacade.GetItem(request.ItemId);
        await _authorizationService.CheckPermissions(item, ItemOperations.Read);

        // Create the loan
        var loan = await _tenantFacade.CreateLoan(request);

        // Generate response
        var response = await _responseGenerator.GenerateLoanDetailResponse(loan);

        // Created response with location header
        return CreatedAtAction(_responseGenerator.GetLink(loan), response);
    }

    /// <summary>
    /// Returns all images of the given item.
    /// </summary>
    /// <param name="loanId">Loan id.</param>
    /// <param name="request">Review request data.</param>
    /// <returns>All images associated with the item.</returns>
    [HttpPost("{loanId}/reviews")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateReview(int loanId, [FromBody] ReviewRequest request)
    {
        // get the loan and check permissions
        var loan = await _loanFacade.GetLoan(loanId);

        // Check permission for the loan.
        await _authorizationService.CheckPermissions(loan, LoanOperations.CreateReview);

        // Create the review
        var review = await _reviewFacade.CreateReview(loan, request);

        // generate response 
        var response = await _reviewResponseGenerator.GenerateReviewDetailResponse(review);

        return CreatedAtAction(_reviewResponseGenerator.GetLink(review), response);
    }
}
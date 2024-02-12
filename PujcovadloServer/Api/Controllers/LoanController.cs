using Microsoft.AspNetCore.Mvc;
using PujcovadloServer.Api.Services;
using PujcovadloServer.AuthorizationHandlers;
using PujcovadloServer.Business.Entities;
using PujcovadloServer.Business.Facades;
using PujcovadloServer.Responses;

namespace PujcovadloServer.Api.Controllers;

public class LoanController : ACrudController<Loan>
{
    private readonly LoanFacade _loanFacade;
    private readonly LoanResponseGenerator _responseGenerator;

    public LoanController(LoanFacade loanFacade, LoanResponseGenerator responseGenerator,
        AuthorizationService authorizationService, LinkGenerator urlHelper) : base(authorizationService, urlHelper)
    {
        _loanFacade = loanFacade;
        _responseGenerator = responseGenerator;
    }

    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LoanResponse>> GetLoan(int id)
    {
        var loan = await _loanFacade.GetLoan(id);

        await _authorizationService.CheckPermissions(loan, LoanAuthorizationHandler.Operations.Read);

        // Generate response
        var response = await _responseGenerator.GenerateLoanDetailResponse(loan);

        return Ok(response);
    }
}
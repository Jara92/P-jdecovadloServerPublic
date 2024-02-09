using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PujcovadloServer.Api.Filters;
using PujcovadloServer.Api.Services;
using PujcovadloServer.AuthorizationHandlers;
using PujcovadloServer.Business.Entities;
using PujcovadloServer.Business.Enums;
using PujcovadloServer.Business.Facades;
using PujcovadloServer.Business.Filters;
using PujcovadloServer.Requests;
using PujcovadloServer.Responses;

namespace PujcovadloServer.Api.Controllers;

[ApiController]
[Route("api/my-owner-loans")]
[Authorize(Roles = UserRoles.Owner)]
[ServiceFilter(typeof(ExceptionFilter))]
public class OwnerLoanController : ACrudController<Loan>
{
    private readonly OwnerFacade _ownerFacade;
    private readonly LoanResponseGenerator _loanResponseGenerator;
    private readonly IMapper _mapper;

    public OwnerLoanController(OwnerFacade loanFacade, LoanResponseGenerator loanResponseGenerator, LinkGenerator urlHelper, IMapper mapper,
        AuthorizationService authorizationService) : base(authorizationService, urlHelper)
    {
        _ownerFacade = loanFacade;
        _loanResponseGenerator = loanResponseGenerator;
        _mapper = mapper;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<List<LoanResponse>>> GetLoans([FromQuery] LoanFilter filter)
    {
        // get my loans
        var loans = await _ownerFacade.GetMyLoans(filter);

        // Generate response list
        var responseLoans = await _loanResponseGenerator.GenerateResponseList(loans, filter, nameof(GetLoans), "OwnerLoan");

        return Ok(responseLoans);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LoanResponse>> GetLoan(int id)
    {
        var loan = await _ownerFacade.GetMyLoan(id);

        await _authorizationService.CheckPermissions(loan, LoanAuthorizationHandler.Operations.Read);
        
        // generate response
        var responseLoan = await _loanResponseGenerator.GenerateLoanDetailResponse(loan);

        return Ok(responseLoan);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ValidateIdFilter]
    public async Task<ActionResult<LoanResponse>> UpdateLoan(int id, [FromBody] OwnerLoanRequest request)
    {
        var loan = await _ownerFacade.GetMyLoan(id);

        await _authorizationService.CheckPermissions(loan, LoanAuthorizationHandler.Operations.Update);
        
        await _ownerFacade.UpdateMyLoan(loan, request);

        return Ok();
    }
}
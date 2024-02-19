using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PujcovadloServer.Api.Filters;
using PujcovadloServer.Api.Services;
using PujcovadloServer.AuthorizationHandlers;
using PujcovadloServer.AuthorizationHandlers.Item;
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
    private readonly LoanFacade _loanFacade;
    private readonly OwnerFacade _ownerFacade;
    private readonly LoanResponseGenerator _loanResponseGenerator;
    private readonly IMapper _mapper;

    public OwnerLoanController(LoanFacade loanFacade, OwnerFacade ownerFacade,
        LoanResponseGenerator loanResponseGenerator, LinkGenerator urlHelper, IMapper mapper,
        AuthorizationService authorizationService) : base(authorizationService, urlHelper)
    {
        _loanFacade = loanFacade;
        _ownerFacade = ownerFacade;
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
        var responseLoans =
            await _loanResponseGenerator.GenerateResponseList(loans, filter, nameof(GetLoans), "OwnerLoan");

        return Ok(responseLoans);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ValidateIdFilter]
    public async Task<ActionResult<LoanResponse>> UpdateLoan(int id, [FromBody] OwnerLoanRequest request)
    {
        var loan = await _loanFacade.GetLoan(id);

        await _authorizationService.CheckPermissions(loan, LoanOperations.Update);

        await _ownerFacade.UpdateMyLoan(loan, request);

        return Ok();
    }
}
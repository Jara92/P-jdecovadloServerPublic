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
[Route("api/my-tenant-loans")]
[Authorize(Roles = UserRoles.Tenant)]
[ServiceFilter(typeof(ExceptionFilter))]
public class TenantLoanController : ACrudController<Loan>
{
    private readonly LoanFacade _loanFacade;
    private readonly TenantFacade _tenantFacade;
    private readonly LoanResponseGenerator _responseGenerator;
    private readonly IMapper _mapper;

    public TenantLoanController(LoanFacade loanFacade, TenantFacade tenantFacade,
        LoanResponseGenerator responseGenerator,
        LinkGenerator urlHelper, IMapper mapper,
        AuthorizationService authorizationService) : base(authorizationService, urlHelper)
    {
        _loanFacade = loanFacade;
        _tenantFacade = tenantFacade;
        _responseGenerator = responseGenerator;
        _mapper = mapper;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<List<LoanResponse>>> GetLoans([FromQuery] LoanFilter filter)
    {
        var loans = await _tenantFacade.GetMyLoans(filter);

        // generate response list
        var response = await _responseGenerator.GenerateResponseList(loans, filter, nameof(GetLoans), "TenantLoan");

        return Ok(response);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ValidateIdFilter]
    public async Task<ActionResult<LoanResponse>> UpdateLoan(int id, [FromBody] TenantLoanRequest request)
    {
        var loan = await _loanFacade.GetLoan(id);

        await _authorizationService.CheckPermissions(loan, LoanAuthorizationHandler.Operations.Update);

        await _tenantFacade.UpdateMyLoan(loan, request);

        return Ok();
    }
}
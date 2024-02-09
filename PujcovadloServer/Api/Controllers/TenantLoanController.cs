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
    private readonly TenantFacade _tenantFacade;
    private readonly LoanResponseGenerator _responseGenerator;
    private readonly IMapper _mapper;

    public TenantLoanController(TenantFacade loanFacade, LoanResponseGenerator responseGenerator,
        LinkGenerator urlHelper, IMapper mapper,
        AuthorizationService authorizationService) : base(authorizationService, urlHelper)
    {
        _tenantFacade = loanFacade;
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

    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LoanResponse>> GetLoan(int id)
    {
        var loan = await _tenantFacade.GetMyLoan(id);

        await _authorizationService.CheckPermissions(loan, LoanAuthorizationHandler.Operations.Read);

        // Generate response
        var response = await _responseGenerator.GenerateLoanDetailResponse(loan);

        return Ok(response);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<LoanResponse>> CreateLoan([FromBody] TenantLoanRequest request)
    {
        await _authorizationService.CheckPermissions(_mapper.Map<Loan>(request),
            LoanAuthorizationHandler.Operations.Create);

        var loan = await _tenantFacade.CreateLoan(request);
        
        // Generate response
        var response = await _responseGenerator.GenerateLoanDetailResponse(loan);

        // Created response with location header
        return CreatedAtAction(_urlHelper.GetUriByAction(HttpContext, nameof(GetLoan), values: loan.Id), response);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ValidateIdFilter]
    public async Task<ActionResult<LoanResponse>> UpdateLoan(int id, [FromBody] TenantLoanRequest request)
    {
        var loan = await _tenantFacade.GetMyLoan(id);

        await _authorizationService.CheckPermissions(loan, LoanAuthorizationHandler.Operations.Update);

        await _tenantFacade.UpdateMyLoan(loan, request);

        return Ok();
    }
}
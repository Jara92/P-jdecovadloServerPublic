using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PujcovadloServer.Api.Filters;
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
    private readonly IMapper _mapper;

    public OwnerLoanController(OwnerFacade loanFacade, LinkGenerator urlHelper, IMapper mapper,
        IAuthorizationService authorizationService) : base(authorizationService, urlHelper)
    {
        _ownerFacade = loanFacade;
        _mapper = mapper;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<List<LoanResponse>>> GetLoans([FromQuery] LoanFilter filter)
    {
        var loans = await _ownerFacade.GetMyLoans(filter);

        var responseLoans = _mapper.Map<List<LoanResponse>>(loans);

        // HATEOS links
        foreach (var loan in responseLoans)
        {
            loan.Links.Add(new LinkResponse(
                _urlHelper.GetUriByAction(HttpContext, nameof(GetLoan), values: new { loan.Id }),
                "SELF", "GET"));

            loan.Links.Add(new LinkResponse(
                _urlHelper.GetUriByAction(HttpContext, nameof(UpdateLoan), values: new { loan.Id }),
                "UPDATE", "PUT"));

            // TODO: uncomment when user controller is ready
            /*loan.Links.Add(new LinkResponse(
                _urlHelper.GetUriByAction(HttpContext, nameof(Get), "User", values: new { loan.Id }),
                "OWNER", "GET"));*/
        }

        // Generate pagination links
        var links = GeneratePaginationLinks(loans, filter, nameof(GetLoans));

        return Ok(new ResponseList<LoanResponse>
        {
            Data = responseLoans,
            Links = links
        });
    }

    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LoanResponse>> GetLoan(int id)
    {
        var loan = await _ownerFacade.GetMyLoan(id);

        await CheckPermissions(loan, LoanAuthorizationHandler.Operations.Read);
        
        var responseLoan = _mapper.Map<LoanResponse>(loan);

        // HATEOS links
        responseLoan.Links.Add(new LinkResponse(
            _urlHelper.GetUriByAction(HttpContext, nameof(GetLoans)), "LIST", "GET"));

        // TODO: uncomment when user controller is ready
        /*responseLoan.Links.Add(new LinkResponse(
            _urlHelper.GetUriByAction(HttpContext, nameof(Get), "User", loan.Tenant.Id), "TENANT", "GET"));

        responseLoan.Links.Add(new LinkResponse(
           _urlHelper.GetUriByAction(HttpContext, nameof(Get), "User", loan.Item.Owner.Id), "OWNER", "GET"));
        */

        responseLoan.Links.Add(new LinkResponse(
            _urlHelper.GetUriByAction(HttpContext, nameof(ItemController.Get), "Item",
                values: new { loan.Item.Id }),
            "ITEM", "GET"));

        return Ok(responseLoan);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ValidateIdFilter]
    public async Task<ActionResult<LoanResponse>> UpdateLoan(int id, [FromBody] TenantLoanRequest request)
    {
        var loan = await _ownerFacade.GetMyLoan(id);

        await CheckPermissions(loan, LoanAuthorizationHandler.Operations.Update);
        
        await _ownerFacade.UpdateMyLoan(loan, request);

        return Ok();
    }
}
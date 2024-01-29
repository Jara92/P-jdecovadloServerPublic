using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PujcovadloServer.Business.Enums;
using PujcovadloServer.Business.Facades;
using PujcovadloServer.Business.Filters;
using PujcovadloServer.Requests;
using PujcovadloServer.Responses;

namespace PujcovadloServer.Api.Controllers;

[ApiController]
[Route("api/my-tenant-loans")]
[Authorize(Roles = UserRoles.Tenant)]
public class TenantLoanController : ACrudController
{
    private readonly TenantFacade _tenantFacade;
    private readonly IMapper _mapper;

    public TenantLoanController(TenantFacade loanFacade, LinkGenerator urlHelper, IMapper mapper) : base(urlHelper)
    {
        _tenantFacade = loanFacade;
        _mapper = mapper;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<List<LoanResponse>>> GetLoans([FromQuery] LoanFilter filter)
    {
        var loans = await _tenantFacade.GetMyLoans(filter);

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
        var loan = await _tenantFacade.GetMyLoan(id);
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

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<LoanResponse>> CreateLoan([FromBody] TenantLoanRequest request)
    {
        // todo: validation not working

        var loan = await _tenantFacade.CreateLoan(request);
        var responseLoan = _mapper.Map<LoanResponse>(loan);

        // Created response with location header
        return CreatedAtAction(_urlHelper.GetUriByAction(HttpContext, nameof(GetLoan), values: loan.Id), responseLoan);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ValidateIdFilter]
    public async Task<ActionResult<LoanResponse>> UpdateLoan(int id, [FromBody] TenantLoanRequest request)
    {
        await _tenantFacade.UpdateMyLoan(request);

        return Ok();
    }
}
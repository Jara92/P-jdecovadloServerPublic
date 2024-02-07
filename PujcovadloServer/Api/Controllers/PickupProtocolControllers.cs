using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PujcovadloServer.Api.Filters;
using PujcovadloServer.AuthorizationHandlers;
using PujcovadloServer.Business.Entities;
using PujcovadloServer.Business.Enums;
using PujcovadloServer.Business.Facades;
using PujcovadloServer.Requests;
using PujcovadloServer.Responses;

namespace PujcovadloServer.Api.Controllers;

[ApiController]
[Route("api/loans/{loanId}/pickup-protocol")]
[Authorize(Roles = UserRoles.Owner)]
[ServiceFilter(typeof(ExceptionFilter))]
public class PickupProtocolControllers : ACrudController<PickupProtocol>
{
    private readonly LoanFacade _loanFacade;
    private readonly IMapper _mapper;

    public PickupProtocolControllers(LoanFacade loanFacade, IAuthorizationService authorizationService,
        LinkGenerator urlHelper, IMapper mapper) : base(authorizationService, urlHelper)
    {
        _loanFacade = loanFacade;
        _mapper = mapper;
    }

    public async Task<ActionResult<PickupProtocolResponse>> GetProtocol(int loanId)
    {
        // Get loan
        var loan = await _loanFacade.GetLoan(loanId);
        await CheckPermissions<Loan>(loan, LoanAuthorizationHandler.Operations.Read);

        // Get protocol
        var protocol = _loanFacade.GetPickupProtocol(loan);
        await CheckPermissions(protocol, PickupProtocolAuthorizationHandler.Operations.Read);

        // build response
        var response = _mapper.Map<PickupProtocolResponse>(protocol);

        // HATEOS links
        // TODO: add loan link later. the problem is that we have two loan controllers for owner and tenant and we need to distinguish them
        /*response.Links.Add(new LinkResponse(
            _urlHelper.GetUriByAction(HttpContext, nameof(), values: new { loan.Id }),
            "LOAN", "GET"));*/

        // Add update link if user has permission
        if (await CanPerformOperation(protocol, PickupProtocolAuthorizationHandler.Operations.Update))
        {
            response.Links.Add(new LinkResponse(
                _urlHelper.GetUriByAction(HttpContext, nameof(UpdateProtocol), values: new { loanId }),
                "UPDATE", "PUT"));
            
            // todo: add images link
            
            // todo: add link for creating a new image
        }

        return Ok(response);
    }

    public async Task<ActionResult<PickupProtocolResponse>> UpdateProtocol(int loanId,
        [FromBody] PickupProtocolRequest request)
    {
        // Get loan
        var loan = await _loanFacade.GetLoan(loanId);
        await CheckPermissions<Loan>(loan, LoanAuthorizationHandler.Operations.Read);

        // Get protocol
        var protocol = _loanFacade.GetPickupProtocol(loan);
        await CheckPermissions(protocol, PickupProtocolAuthorizationHandler.Operations.Update);

        // Update protocol
        await _loanFacade.UpdatePickupProtocol(protocol, request);

        // build response
        var response = _mapper.Map<PickupProtocolResponse>(protocol);

        // HATEOS links
        // TODO: LINKS

        return Ok(response);
    }
}
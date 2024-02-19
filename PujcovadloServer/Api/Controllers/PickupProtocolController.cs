using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using PujcovadloServer.Api.Filters;
using PujcovadloServer.Api.Services;
using PujcovadloServer.AuthorizationHandlers;
using PujcovadloServer.AuthorizationHandlers.Item;
using PujcovadloServer.AuthorizationHandlers.PickupProtocol;
using PujcovadloServer.Business.Entities;
using PujcovadloServer.Business.Exceptions;
using PujcovadloServer.Business.Facades;
using PujcovadloServer.Requests;
using PujcovadloServer.Responses;

namespace PujcovadloServer.Api.Controllers;

[ApiController]
[Route("api/loans/{loanId}/pickup-protocol")]
[ServiceFilter(typeof(ExceptionFilter))]
public class PickupProtocolController : ACrudController<PickupProtocol>
{
    private readonly LoanFacade _loanFacade;
    private readonly PickupProtocolFacade _pickupProtocolFacade;
    private readonly PickupProtocolResponseGenerator _responseGenerator;
    private readonly IMapper _mapper;

    public PickupProtocolController(LoanFacade loanFacade, PickupProtocolFacade pickupProtocolFacade,
        PickupProtocolResponseGenerator responseGenerator,
        AuthorizationService authorizationService,
        LinkGenerator urlHelper, IMapper mapper) : base(authorizationService, urlHelper)
    {
        _loanFacade = loanFacade;
        _pickupProtocolFacade = pickupProtocolFacade;
        _responseGenerator = responseGenerator;
        _mapper = mapper;
    }

    /// <summary>
    /// Returns pickup protocol for the loan.
    /// </summary>
    /// <param name="loanId">Id of the loan</param>
    /// <returns>Pickup protocol.</returns>
    /// <response code="200">Returns pickup protocol.</response>
    /// <response code="403">User is not allowed to read the pickup protocol.</response>
    /// <response code="404">Loan not found.</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PickupProtocolResponse>> GetProtocol(int loanId)
    {
        // Get loan
        var loan = await _loanFacade.GetLoan(loanId);
        await _authorizationService.CheckPermissions(loan, LoanOperations.Read);

        // Get protocol
        var protocol = _pickupProtocolFacade.GetPickupProtocol(loan);
        await _authorizationService.CheckPermissions(protocol, PickupProtocolOperations.Read);

        // build response
        var response = await _responseGenerator.GeneratePickupProtocolDetailResponse(protocol);

        return Ok(response);
    }

    /// <summary>
    /// Updates pickup protocol for the loan.
    /// </summary>
    /// <param name="loanId">Loan id</param>
    /// <param name="request">Pickup protocol data.</param>
    /// <returns></returns>
    /// <response code="204">Pickup protocol updated.</response>
    /// <response code="400">Invalid input data or action is not allowed.</response>
    /// <response code="403">User is not allowed to update the pickup protocol.</response>
    /// <response code="404">Loan not found.</response>
    [HttpPut]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PickupProtocolResponse>> UpdateProtocol(int loanId,
        [FromBody] PickupProtocolRequest request)
    {
        // Get loan
        var loan = await _loanFacade.GetLoan(loanId);
        await _authorizationService.CheckPermissions(loan, LoanOperations.Read);

        // Get protocol and update it if it exists
        try
        {
            var protocol = _pickupProtocolFacade.GetPickupProtocol(loan);
            await _authorizationService.CheckPermissions(protocol,
                PickupProtocolOperations.Update);
            await _pickupProtocolFacade.UpdatePickupProtocol(protocol, request);

            return NoContent();
        }
        // Protocol does not exist, try to create it
        catch (EntityNotFoundException)
        {
            await _authorizationService.CheckPermissions(loan,
                LoanOperations.CreatePickupProtocol);
            var protocol = await _pickupProtocolFacade.CreatePickupProtocol(loan, request);

            // generate response
            var response = await _responseGenerator.GeneratePickupProtocolDetailResponse(protocol);

            return CreatedAtAction(_urlHelper.GetUriByAction(HttpContext, nameof(GetProtocol), values: protocol.Id),
                response);
        }
    }
}
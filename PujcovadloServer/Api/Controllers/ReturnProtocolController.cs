using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using PujcovadloServer.Api.Filters;
using PujcovadloServer.Api.Services;
using PujcovadloServer.AuthorizationHandlers;
using PujcovadloServer.Business.Entities;
using PujcovadloServer.Business.Exceptions;
using PujcovadloServer.Business.Facades;
using PujcovadloServer.Requests;
using PujcovadloServer.Responses;

namespace PujcovadloServer.Api.Controllers;

[ApiController]
[Route("api/loans/{loanId}/return-protocol")]
[ServiceFilter(typeof(ExceptionFilter))]
public class ReturnProtocolController : ACrudController<ReturnProtocol>
{
    private readonly LoanFacade _loanFacade;
    private readonly ReturnProtocolFacade _returnProtocolFacade;
    private readonly ReturnProtocolResponseGenerator _responseGenerator;
    private readonly IMapper _mapper;

    public ReturnProtocolController(LoanFacade loanFacade, ReturnProtocolFacade returnProtocolFacade,
        ReturnProtocolResponseGenerator responseGenerator,
        AuthorizationService authorizationService,
        LinkGenerator urlHelper, IMapper mapper) : base(authorizationService, urlHelper)
    {
        _loanFacade = loanFacade;
        _returnProtocolFacade = returnProtocolFacade;
        _responseGenerator = responseGenerator;
        _mapper = mapper;
    }

    /// <summary>
    /// Returns pickup protocol for the loan.
    /// </summary>
    /// <param name="loanId">Id of the loan</param>
    /// <returns>Return protocol.</returns>
    /// <response code="200">Returns pickup protocol.</response>
    /// <response code="403">User is not allowed to read the return protocol.</response>
    /// <response code="404">Loan not found.</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReturnProtocolResponse>> GetProtocol(int loanId)
    {
        // Get loan
        var loan = await _loanFacade.GetLoan(loanId);
        await _authorizationService.CheckPermissions(loan, LoanAuthorizationHandler.Operations.Read);

        // Get protocol
        var protocol = _returnProtocolFacade.GetReturnProtocol(loan);
        await _authorizationService.CheckPermissions(protocol, ReturnProtocolAuthorizationHandler.Operations.Read);

        // build response
        var response = await _responseGenerator.GenerateReturnProtocolDetailResponse(protocol);

        return Ok(response);
    }

    /// <summary>
    /// Updates pickup protocol for the loan.
    /// </summary>
    /// <param name="loanId">Loan id</param>
    /// <param name="request">Return protocol data.</param>
    /// <returns></returns>
    /// <response code="204">Return protocol updated.</response>
    /// <response code="400">Invalid input data or action is not allowed.</response>
    /// <response code="403">User is not allowed to update the return protocol.</response>
    /// <response code="404">Loan not found.</response>
    [HttpPut]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReturnProtocolResponse>> UpdateProtocol(int loanId,
        [FromBody] ReturnProtocolRequest request)
    {
        // Get loan
        var loan = await _loanFacade.GetLoan(loanId);
        await _authorizationService.CheckPermissions(loan, LoanAuthorizationHandler.Operations.Read);

        // Get protocol and update it if it exists
        try
        {
            var protocol = _returnProtocolFacade.GetReturnProtocol(loan);
            await _authorizationService.CheckPermissions(protocol,
                ReturnProtocolAuthorizationHandler.Operations.Update);
            await _returnProtocolFacade.UpdateReturnProtocol(protocol, request);

            return NoContent();
        }
        // Protocol does not exist, try to create it
        catch (EntityNotFoundException)
        {
            await _authorizationService.CheckPermissions(loan,
                LoanAuthorizationHandler.Operations.CreateReturnProtocol);
            var protocol = await _returnProtocolFacade.CreateReturnProtocol(loan, request);

            // generate response
            var response = await _responseGenerator.GenerateReturnProtocolDetailResponse(protocol);

            return CreatedAtAction(_urlHelper.GetUriByAction(HttpContext, nameof(GetProtocol), values: protocol.Id),
                response);
        }
    }
}
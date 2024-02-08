using System.Security.Authentication;
using AutoMapper;
using PujcovadloServer.Business.Entities;
using PujcovadloServer.Business.Enums;
using PujcovadloServer.Business.Exceptions;
using PujcovadloServer.Business.Filters;
using PujcovadloServer.Business.Services;
using PujcovadloServer.Business.Services.Interfaces;
using PujcovadloServer.Lib;
using PujcovadloServer.Requests;

namespace PujcovadloServer.Business.Facades;

public class OwnerFacade
{
    private readonly LoanService _loanService;
    private readonly ItemService _itemService;
    private readonly PickupProtocolService _pickupProtocolService;
    private readonly IAuthenticateService _authenticateService;
    private readonly IMapper _mapper;

    public OwnerFacade(LoanService loanService, ItemService itemService, PickupProtocolService pickupProtocolService,
        IAuthenticateService authenticateService, IMapper mapper)
    {
        _loanService = loanService;
        _itemService = itemService;
        _pickupProtocolService = pickupProtocolService;
        _authenticateService = authenticateService;
        _mapper = mapper;
    }

    public async Task<PaginatedList<Loan>> GetMyLoans(LoanFilter filter)
    {
        // Get current user
        var user = await _authenticateService.GetCurrentUser();

        // Get loans where the user is the tenant
        var loans = await _loanService.GetLoansByOwner(user, filter);

        return loans;
    }

    public async Task<Loan> GetMyLoan(int id)
    {
        // Get current user
        var userId = _authenticateService.GetCurrentUserId();

        // Get the loan
        var loan = await _loanService.Get(id);

        // Check that the loan exists
        if (loan == null) throw new EntityNotFoundException();

        // Check that the user is the tenant
        if (loan.Item.Owner.Id != userId) throw new UnauthorizedAccessException("You are not the owner of this loan.");

        return loan;
    }

    public async Task UpdateMyLoan(Loan loan, OwnerLoanRequest request)
    {
        // Check if the status has been changed
        if (request.Status != null && request.Status != loan.Status)
        {
            // get current state
            var state = _loanService.GetState(loan);
            
            // handle the request
            state.HandleOwner(loan, request.Status.Value);
        }

        /*
        TODO: Change loan parameters
        In the future it would be great to have the option to change date, note etc.
        But these changes are possible to be made only if the loan is in some specific statuses.
        */

        await _loanService.Update(loan);
    }

    /// <summary>
    /// Creates a pickup protocol for the loan.
    /// </summary>
    /// <param name="loan">Protocols loan.</param>
    /// <param name="request">Protocol request data.</param>
    /// <returns>Returns newly created pickup protocol</returns>
    /// <exception cref="ActionNotAllowedException">Thrown if pickup protocol is not allowed to be created.</exception>
    public async Task<PickupProtocol> CreatePickupProtocol(Loan loan, PickupProtocolRequest request)
    {
        // Check if the loan is in the correct status
        if (loan.Status != LoanStatus.PreparedForPickup)
            throw new ActionNotAllowedException("Cannot create pickup protocol for loan in status " + loan.Status);
        
        // Check if the protocol already exists
        if(loan.PickupProtocol != null)
            throw new ActionNotAllowedException("Pickup protocol already exists.");

        // Create protocol
        var protocol = _mapper.Map<PickupProtocol>(request);
        protocol.Loan = loan;

        // Save protocol
        await _pickupProtocolService.Create(protocol);

        return protocol;
    }
    
    public async Task UpdatePickupProtocol(PickupProtocol protocol, PickupProtocolRequest request)
    {
        protocol.Description = request.Description;
        protocol.AcceptedRefundableDeposit = request.AcceptedRefundableDeposit;
        
        await _pickupProtocolService.Update(protocol);
    }
}
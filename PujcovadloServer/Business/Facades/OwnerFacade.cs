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
    private readonly ImageFacade _imageFacade;
    private readonly LoanService _loanService;
    private readonly ItemService _itemService;
    private readonly PickupProtocolService _pickupProtocolService;
    private readonly IAuthenticateService _authenticateService;
    private readonly IMapper _mapper;
    private readonly PujcovadloServerConfiguration _configuration;

    public OwnerFacade(ImageFacade imageFacade, LoanService loanService, ItemService itemService,
        PickupProtocolService pickupProtocolService,
        IAuthenticateService authenticateService, IMapper mapper, PujcovadloServerConfiguration configuration)
    {
        _imageFacade = imageFacade;
        _loanService = loanService;
        _itemService = itemService;
        _pickupProtocolService = pickupProtocolService;
        _authenticateService = authenticateService;
        _mapper = mapper;
        _configuration = configuration;
    }

    public async Task<PaginatedList<Loan>> GetMyLoans(LoanFilter filter)
    {
        // Get current user
        var user = await _authenticateService.GetCurrentUser();

        // Get loans where the user is the tenant
        var loans = await _loanService.GetLoansByOwner(user, filter);

        return loans;
    }

    public async Task UpdateMyLoan(Loan loan, OwnerLoanRequest request)
    {
        // Check if the status has been changed
        if (request.Status != null)
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
        if (loan.Status != LoanStatus.Accepted)
            throw new OperationNotAllowedException("Loan must be in status " + LoanStatus.Accepted +
                                                   " to create pickup protocol.");

        // Check if the protocol already exists
        if (loan.PickupProtocol != null)
            throw new ActionNotAllowedException("Pickup protocol already exists.");

        // Create protocol
        var protocol = _mapper.Map<PickupProtocol>(request);
        protocol.Loan = loan;

        // Create the protocol
        await _pickupProtocolService.Create(protocol);

        return protocol;
    }

    /// <summary>
    /// Updated PickupProtocol.
    /// </summary>
    /// <param name="protocol">Pickup protocol to update.</param>
    /// <param name="request">New data</param>
    /// <exception cref="ActionNotAllowedException">Thrown if the action cannot be performed.</exception>
    public async Task UpdatePickupProtocol(PickupProtocol protocol, PickupProtocolRequest request)
    {
        // Check if the protocol can be updated
        if (protocol.Loan.Status != LoanStatus.PickupDenied)
            throw new ActionNotAllowedException("Pickup protocol can be updated only if the loan pickup is denied.");

        // Update protocol data
        protocol.Description = request.Description;
        protocol.AcceptedRefundableDeposit = request.AcceptedRefundableDeposit;

        await _pickupProtocolService.Update(protocol);
    }

    public async Task AddPickupProtocolImage(PickupProtocol pickupProtocol, Image image, string filePath)
    {
        // Check that the item has not reached the maximum number of images
        if (pickupProtocol.Images.Count >= _configuration.MaxImagesPerPickupProtocol)
            throw new ArgumentException("Max images per pickupProtocol exceeded.");

        // Check loan status
        if (pickupProtocol.Loan.Status != LoanStatus.Accepted && pickupProtocol.Loan.Status != LoanStatus.PickupDenied)
            throw new ActionNotAllowedException(
                "Images can be added only if the loan is accepted or pickup is denied.");

        // set images pickupProtocol
        image.PickupProtocol = pickupProtocol;

        // Create using image facade
        await _imageFacade.Create(image, filePath);
    }
}
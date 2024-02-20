using AutoMapper;
using PujcovadloServer.Business.Entities;
using PujcovadloServer.Business.Exceptions;
using PujcovadloServer.Business.Services;
using PujcovadloServer.Requests;

namespace PujcovadloServer.Business.Facades;

public class ReturnProtocolFacade
{
    private readonly ImageFacade _imageFacade;
    private readonly ReturnProtocolService _returnProtocolService;
    private readonly LoanService _loanService;
    private readonly IMapper _mapper;
    private readonly PujcovadloServerConfiguration _configuration;

    public ReturnProtocolFacade(ImageFacade imageFacade, ReturnProtocolService returnProtocolService,
        LoanService loanService, IMapper mapper, PujcovadloServerConfiguration configuration)
    {
        _imageFacade = imageFacade;
        _returnProtocolService = returnProtocolService;
        _loanService = loanService;
        _mapper = mapper;
        _configuration = configuration;
    }

    /// <summary>
    /// Returns pickup protocol for the loan.
    /// </summary>
    /// <param name="loan">The loan.</param>
    /// <returns>Return protocol of the loan</returns>
    /// <exception cref="EntityNotFoundException">Thrown when the loan has no pickup protocol.</exception>
    public ReturnProtocol GetReturnProtocol(Loan loan)
    {
        var protocol = loan.ReturnProtocol;
        if (protocol == null) throw new EntityNotFoundException("Return protocol not found");

        return protocol;
    }

    /// <summary>
    /// tests if the protocol for a loan can be created.
    /// </summary>
    /// <param name="loan">The loan</param>
    /// <exception cref="OperationNotAllowedException">Thrown is the protocol cannot be created.</exception>
    public (bool CanCreate, string Reason) CanCreateReturnProtocol(Loan loan)
    {
        // Check if the loan state allows the protocol to be created
        var loanState = _loanService.GetState(loan);
        if (loanState.CanCreateReturnProtocol(loan) == false)
            return (false, "Return protocol cannot be created in the current loan status.");

        // Check if the protocol already exists
        if (loan.ReturnProtocol != null)
            return (false, "Return protocol already exists");

        return (true, "OK");
    }

    /// <summary>
    /// Creates a pickup protocol for the loan.
    /// </summary>
    /// <param name="loan">Protocols loan.</param>
    /// <param name="request">Protocol request data.</param>
    /// <returns>Returns newly created pickup protocol</returns>
    /// <exception cref="OperationNotAllowedException">Thrown if pickup protocol is not allowed to be created.</exception>
    public async Task<ReturnProtocol> CreateReturnProtocol(Loan loan, ReturnProtocolRequest request)
    {
        // Check if the protocol can be created
        var canCreateResult = CanCreateReturnProtocol(loan);
        if (canCreateResult.CanCreate == false)
            throw new OperationNotAllowedException(canCreateResult.Reason);

        // Create protocol
        var protocol = _mapper.Map<ReturnProtocol>(request);
        protocol.Loan = loan;

        // Create the protocol
        await _returnProtocolService.Create(protocol);

        return protocol;
    }

    /// <summary>
    /// Tests if the protocol can be updated.
    /// </summary>
    /// <param name="protocol">Protocol to be updated.</param>
    /// <returns>CanUpdate = true if the protocol can be updated.</returns>
    /// <exception cref="OperationNotAllowedException">Thrown is the protocol cannot be created.</exception>
    public (bool CanUpdate, string Reason) CanUpdateReturnProtocol(ReturnProtocol protocol)
    {
        // Check if the loan state allows the protocol to be updated
        var loanState = _loanService.GetState(protocol.Loan);
        if (loanState.CanUpdateReturnProtocol(protocol.Loan) == false)
            return (false, "Return protocol cannot be updated in the current loan status.");

        return (true, "OK");
    }

    /// <summary>
    /// Updated ReturnProtocol.
    /// </summary>
    /// <param name="protocol">Return protocol to update.</param>
    /// <param name="request">New data</param>
    /// <exception cref="OperationNotAllowedException">Thrown if the action cannot be performed.</exception>
    public async Task UpdateReturnProtocol(ReturnProtocol protocol, ReturnProtocolRequest request)
    {
        // Check if the protocol can be updated
        var canUpdateResult = CanUpdateReturnProtocol(protocol);
        if (canUpdateResult.CanUpdate == false)
            throw new OperationNotAllowedException(canUpdateResult.Reason);

        // Update protocol data
        protocol.Description = request.Description;
        protocol.ReturnedRefundableDeposit = request.ReturnedRefundableDeposit;

        await _returnProtocolService.Update(protocol);
    }

    public async Task AddReturnProtocolImage(ReturnProtocol returnProtocol, Image image, string filePath)
    {
        // Check if the protocol can be updated
        var canUpdateResult = CanUpdateReturnProtocol(returnProtocol);
        if (canUpdateResult.CanUpdate == false)
            throw new OperationNotAllowedException(canUpdateResult.Reason);

        // Check that the item has not reached the maximum number of images
        if (returnProtocol.Images.Count >= _configuration.MaxImagesPerReturnProtocol)
            throw new ArgumentException("Max images per returnProtocol exceeded.");

        // set images returnProtocol
        image.ReturnProtocol = returnProtocol;

        // Create using image facade
        await _imageFacade.Create(image, filePath);
    }
}
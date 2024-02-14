using AutoMapper;
using PujcovadloServer.Business.Entities;
using PujcovadloServer.Business.Enums;
using PujcovadloServer.Business.Exceptions;
using PujcovadloServer.Business.Services;
using PujcovadloServer.Requests;

namespace PujcovadloServer.Business.Facades;

public class ReturnProtocolFacade
{
    private readonly ImageFacade _imageFacade;
    private readonly ReturnProtocolService _returnProtocolService;
    private readonly IMapper _mapper;
    private readonly PujcovadloServerConfiguration _configuration;

    public ReturnProtocolFacade(ImageFacade imageFacade, ReturnProtocolService returnProtocolService, IMapper mapper,
        PujcovadloServerConfiguration configuration)
    {
        _imageFacade = imageFacade;
        _returnProtocolService = returnProtocolService;
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
    public void CheckCanCreateReturnProtocol(Loan loan)
    {
        // Check if the loan is in the correct status
        if (loan.Status != LoanStatus.Active)
            throw new OperationNotAllowedException("Loan must be in status " + LoanStatus.Active +
                                                   " to create pickup protocol.");

        // Check if the protocol already exists
        if (loan.ReturnProtocol != null)
            throw new OperationNotAllowedException("Return protocol already exists.");
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
        CheckCanCreateReturnProtocol(loan);

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
    /// <exception cref="OperationNotAllowedException">Thrown is the protocol cannot be created.</exception>
    public void CheckCanUpdateReturnProtocol(ReturnProtocol protocol)
    {
        if (protocol.Loan.Status != LoanStatus.Active && protocol.Loan.Status != LoanStatus.ReturnDenied)
            throw new OperationNotAllowedException(
                "Return protocol can be updated only if the loan is active or returns is denied.");
    }

    /// <summary>
    /// Updated ReturnProtocol.
    /// </summary>
    /// <param name="protocol">Return protocol to update.</param>
    /// <param name="request">New data</param>
    /// <exception cref="OperationNotAllowedException">Thrown if the action cannot be performed.</exception>
    public async Task UpdateReturnProtocol(ReturnProtocol protocol, ReturnProtocolRequest request)
    {
        CheckCanUpdateReturnProtocol(protocol);

        // Update protocol data
        protocol.Description = request.Description;
        protocol.ReturnedRefundableDeposit = request.ReturnedRefundableDeposit;

        await _returnProtocolService.Update(protocol);
    }

    public async Task AddReturnProtocolImage(ReturnProtocol returnProtocol, Image image, string filePath)
    {
        CheckCanUpdateReturnProtocol(returnProtocol);

        // Check that the item has not reached the maximum number of images
        if (returnProtocol.Images.Count >= _configuration.MaxImagesPerReturnProtocol)
            throw new ArgumentException("Max images per returnProtocol exceeded.");

        // set images returnProtocol
        image.ReturnProtocol = returnProtocol;

        // Create using image facade
        await _imageFacade.Create(image, filePath);
    }
}
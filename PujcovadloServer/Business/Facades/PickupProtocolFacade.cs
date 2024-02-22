using AutoMapper;
using PujcovadloServer.Business.Entities;
using PujcovadloServer.Business.Exceptions;
using PujcovadloServer.Business.Services;
using PujcovadloServer.Requests;

namespace PujcovadloServer.Business.Facades;

public class PickupProtocolFacade
{
    private readonly ImageFacade _imageFacade;
    private readonly PickupProtocolService _pickupProtocolService;
    private readonly LoanService _loanService;
    private readonly IMapper _mapper;
    private readonly PujcovadloServerConfiguration _configuration;

    public PickupProtocolFacade(ImageFacade imageFacade, PickupProtocolService pickupProtocolService,
        LoanService loanService,
        IMapper mapper, PujcovadloServerConfiguration configuration)
    {
        _imageFacade = imageFacade;
        _pickupProtocolService = pickupProtocolService;
        _loanService = loanService;
        _mapper = mapper;
        _configuration = configuration;
    }

    /// <summary>
    /// Returns pickup protocol for the loan.
    /// </summary>
    /// <param name="loan">The loan.</param>
    /// <returns>Pickup protocol of the loan</returns>
    /// <exception cref="EntityNotFoundException">Thrown when the loan has no pickup protocol.</exception>
    public PickupProtocol GetPickupProtocol(Loan loan)
    {
        var protocol = loan.PickupProtocol;
        if (protocol == null) throw new EntityNotFoundException("Pickup protocol not found");

        return protocol;
    }

    /// <summary>
    /// Can the pickup protocol be created?
    /// </summary>
    /// <param name="loan">Loan of the pickup protocol</param>
    /// <returns>CanCreate = if the pickup protocol can be created.</returns>
    public (bool CanCreate, string Reason) CanCreatePickupProtocol(Loan loan)
    {
        // Get current loan status
        var loanState = _loanService.GetState(loan);

        // Check if the protocol can be created in the current loan status
        if (loanState.CanCreatePickupProtocol(loan) == false)
            return (false, "Pickup protocol cannot be created in the current loan status.");

        // Check if the protocol already exists
        if (loan.PickupProtocol != null)
            return (false, "Pickup protocol already exists.");

        return (true, "OK");
    }

    /// <summary>
    /// Creates a pickup protocol for the loan.
    /// </summary>
    /// <param name="loan">Protocols loan.</param>
    /// <param name="request">Protocol request data.</param>
    /// <returns>Returns newly created pickup protocol</returns>
    /// <exception cref="OperationNotAllowedException">Thrown if pickup protocol is not allowed to be created.</exception>
    public async Task<PickupProtocol> CreatePickupProtocol(Loan loan, PickupProtocolRequest request)
    {
        // Check if the protocol can be created
        var canCreateResult = CanCreatePickupProtocol(loan);
        if (canCreateResult.CanCreate == false)
            throw new OperationNotAllowedException(canCreateResult.Reason);

        // Create protocol
        var protocol = _mapper.Map<PickupProtocol>(request);
        protocol.Loan = loan;

        // Create the protocol
        await _pickupProtocolService.Create(protocol);

        return protocol;
    }

    /// <summary>
    /// Can the pickup protocol be updated?
    /// </summary>
    /// <param name="protocol"></param>
    /// <returns>CanUpdate = true if the pickup protocol can be updated.</returns>
    public (bool CanUpdate, string Reason) CanUpdatePickupProtocol(PickupProtocol protocol)
    {
        // Get current loan status
        var loanState = _loanService.GetState(protocol.Loan);

        // Check if the protocol can be updated
        if (loanState.CanUpdatePickupProtocol(protocol.Loan) == false)
        {
            return (false, "Pickup protocol cannot be updated in the current loan status.");
        }

        return (true, "OK");
    }

    /// <summary>
    /// Updated PickupProtocol.
    /// </summary>
    /// <param name="protocol">Pickup protocol to update.</param>
    /// <param name="request">New data</param>
    /// <exception cref="OperationNotAllowedException">Thrown if the action cannot be performed.</exception>
    public async Task UpdatePickupProtocol(PickupProtocol protocol, PickupProtocolRequest request)
    {
        // Check if the protocol can be updated
        var canUpdateResult = CanUpdatePickupProtocol(protocol);
        if (canUpdateResult.CanUpdate == false)
            throw new OperationNotAllowedException(canUpdateResult.Reason);

        // Update protocol data
        protocol.Description = request.Description;
        protocol.AcceptedRefundableDeposit = request.AcceptedRefundableDeposit;

        await _pickupProtocolService.Update(protocol);
    }

    /// <summary>
    /// Add image to the pickup protocol.
    /// </summary>
    /// <param name="pickupProtocol">Pickup protocol to be updated.</param>
    /// <param name="image">Image to be added</param>
    /// <param name="filePath">Filepath of the image file.</param>
    /// <exception cref="OperationNotAllowedException">Thrown when the protocol cannot be updated.</exception>
    /// <exception cref="ArgumentException">Thrown when maximum amount of images was exceeded</exception>
    public async Task AddImage(PickupProtocol pickupProtocol, Image image, string filePath)
    {
        // Check that the item can be updated
        var canUpdateResult = CanUpdatePickupProtocol(pickupProtocol);
        if (canUpdateResult.CanUpdate == false)
            throw new OperationNotAllowedException(canUpdateResult.Reason);

        // Check that the item has not reached the maximum number of images
        if (pickupProtocol.Images.Count >= _configuration.MaxImagesPerPickupProtocol)
            throw new ArgumentException("Max images per pickupProtocol exceeded.");

        // set image pickupProtocol
        image.PickupProtocol = pickupProtocol;

        // Create using image facade
        await _imageFacade.CreateImage(image, filePath);

        // Update pickupProtocol
        await _pickupProtocolService.Update(pickupProtocol);
    }

    /// <summary>
    /// Returns image of the pickup protocol.
    /// </summary>
    /// <param name="pickupProtocolId">PickupProtocol id</param>
    /// <param name="imageId">Image id</param>
    /// <returns>Image</returns>
    /// <exception cref="EntityNotFoundException">Thrown when image with given imageId or pickupProtocolId does not exist</exception>
    public async Task<Image> GetImage(int pickupProtocolId, int imageId)
    {
        // Get the image by id using the image facade
        var image = await _imageFacade.GetImage(imageId);

        // Check that the image belongs to the pickup protocol
        if (image.PickupProtocol == null || image.PickupProtocol.Id != pickupProtocolId)
        {
            throw new EntityNotFoundException("Image not found");
        }

        return image;
    }

    /// <summary>
    /// Deletes given image which belongs to a pickup protocol.
    /// </summary>
    /// <param name="image">Image to be deleted</param>
    /// <returns></returns>
    /// <exception cref="OperationNotAllowedException">Thrown if the image doest belong to a pickup protocol or if the image cannot be deleted.</exception>
    public async Task DeleteImage(Image image)
    {
        // Check that the image belongs to the pickup protocol
        if (image.PickupProtocol == null)
        {
            throw new OperationNotAllowedException("Image does not belong to any pickup protocol");
        }

        // Check if the protocol can be updated
        var canUpdateResult = CanUpdatePickupProtocol(image.PickupProtocol);
        if (canUpdateResult.CanUpdate == false)
            throw new OperationNotAllowedException(canUpdateResult.Reason);

        // Delete the image
        await _imageFacade.DeleteImage(image);

        // Update the protocol
        await _pickupProtocolService.Update(image.PickupProtocol);
    }
}
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
    private readonly ImageResponseGenerator _imageResponseGenerator;
    private readonly FileUploadService _fileUploadService;
    private readonly IMapper _mapper;

    public PickupProtocolController(LoanFacade loanFacade, PickupProtocolFacade pickupProtocolFacade,
        PickupProtocolResponseGenerator responseGenerator, ImageResponseGenerator imageResponseGenerator,
        FileUploadService fileUploadService, AuthorizationService authorizationService,
        LinkGenerator urlHelper, IMapper mapper) : base(authorizationService, urlHelper)
    {
        _loanFacade = loanFacade;
        _pickupProtocolFacade = pickupProtocolFacade;
        _responseGenerator = responseGenerator;
        _imageResponseGenerator = imageResponseGenerator;
        _fileUploadService = fileUploadService;
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

    /// <summary>
    /// Returns all images of the given pickup protocol.
    /// </summary>
    /// <param name="loanId">Loan loanId.</param>
    /// <returns>All images associated with the item.</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetImages(int loanId)
    {
        // get the item and check permissions
        var loan = await _loanFacade.GetLoan(loanId);

        // Check permission for the loan.
        await _authorizationService.CheckPermissions(loan, LoanOperations.Read);

        // get pickup protocol instance
        var pickupProtocol = _pickupProtocolFacade.GetPickupProtocol(loan);

        // Check permissions for the pickup protocol
        await _authorizationService.CheckPermissions(pickupProtocol,
            PickupProtocolOperations.Read);

        // get the images and map them to response
        var images = pickupProtocol.Images;

        // generate response 
        var response = await _imageResponseGenerator.GenerateResponseList(images);

        return Ok(response);
    }

    /// <summary>
    /// Creates new image for the given pickup protocol.
    /// </summary>
    /// <param name="loanId">Item loanId.</param>
    /// <param name="file">Image file.</param>
    /// <returns>Newly create image.</returns>
    /// <response code="201">Returns newly created image.</response>
    /// <response code="400">If the request is not valid.</response>
    /// <response code="403">If the user is not authorized to create the image.</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> AddImage(int loanId, IFormFile file)
    {
        // get the item and check permissions
        var loan = await _loanFacade.GetLoan(loanId);

        // Verify that the user can read the loan data
        await _authorizationService.CheckPermissions(loan, LoanOperations.Read);

        // get pickup protocol instance
        var pickupProtocol = _pickupProtocolFacade.GetPickupProtocol(loan);

        // Check permissions for creating images of the pickup protocol
        await _authorizationService.CheckPermissions(pickupProtocol,
            PickupProtocolOperations.Update);

        // Save the image to the file system
        var filePath = await _fileUploadService.SaveUploadedImage(file);

        // Create new image
        var image = new Image()
        {
            Name = file.FileName,
            Extension = _fileUploadService.GetFileExtension(file),
            MimeType = _fileUploadService.GetMimeType(file),
            PickupProtocol = pickupProtocol
        };

        await _authorizationService.CheckPermissions(image, PickupProtocolOperations.Update);

        // Save the image to the database
        await _pickupProtocolFacade.AddPickupProtocolImage(pickupProtocol, image, filePath);

        // Map the image to response
        var response = await _imageResponseGenerator.GenerateImageDetailResponse(image);

        return Created(_imageResponseGenerator.GetLink(image), response);
    }
}
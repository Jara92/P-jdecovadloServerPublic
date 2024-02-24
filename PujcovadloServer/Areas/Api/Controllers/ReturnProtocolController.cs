using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using PujcovadloServer.Areas.Api.Filters;
using PujcovadloServer.Areas.Api.Services;
using PujcovadloServer.AuthorizationHandlers;
using PujcovadloServer.AuthorizationHandlers.Loan;
using PujcovadloServer.AuthorizationHandlers.ReturnProtocol;
using PujcovadloServer.Business.Entities;
using PujcovadloServer.Business.Exceptions;
using PujcovadloServer.Business.Facades;
using PujcovadloServer.Requests;
using PujcovadloServer.Responses;

namespace PujcovadloServer.Areas.Api.Controllers;

[Area("Api")]
[ApiController]
[Route("api/loans/{loanId}/return-protocol")]
[ServiceFilter(typeof(ExceptionFilter))]
public class ReturnProtocolController : ACrudController<ReturnProtocol>
{
    private readonly LoanFacade _loanFacade;
    private readonly ReturnProtocolFacade _returnProtocolFacade;
    private readonly ReturnProtocolResponseGenerator _responseGenerator;
    private readonly ImageResponseGenerator _imageResponseGenerator;
    private readonly FileUploadService _fileUploadService;
    private readonly IMapper _mapper;

    public ReturnProtocolController(LoanFacade loanFacade, ReturnProtocolFacade returnProtocolFacade,
        ReturnProtocolResponseGenerator responseGenerator, ImageResponseGenerator imageResponseGenerator,
        FileUploadService fileUploadService, AuthorizationService authorizationService,
        LinkGenerator urlHelper, IMapper mapper) : base(authorizationService, urlHelper)
    {
        _loanFacade = loanFacade;
        _returnProtocolFacade = returnProtocolFacade;
        _responseGenerator = responseGenerator;
        _imageResponseGenerator = imageResponseGenerator;
        _fileUploadService = fileUploadService;
        _mapper = mapper;
    }

    /// <summary>
    /// Returns return protocol for the loan.
    /// </summary>
    /// <param name="loanId">Id of the loan</param>
    /// <returns>Return protocol.</returns>
    /// <response code="200">Returns return protocol.</response>
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
        await _authorizationService.CheckPermissions(loan, LoanOperations.Read);

        // Get protocol
        var protocol = _returnProtocolFacade.GetReturnProtocol(loan);
        await _authorizationService.CheckPermissions(protocol, ReturnProtocolOperations.Read);

        // build response
        var response = await _responseGenerator.GenerateReturnProtocolDetailResponse(protocol);

        return Ok(response);
    }

    /// <summary>
    /// Updates return protocol for the loan.
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
        await _authorizationService.CheckPermissions(loan, LoanOperations.Read);

        // Get protocol and update it if it exists
        try
        {
            var protocol = _returnProtocolFacade.GetReturnProtocol(loan);
            await _authorizationService.CheckPermissions(protocol,
                ReturnProtocolOperations.Update);
            await _returnProtocolFacade.UpdateReturnProtocol(protocol, request);

            return NoContent();
        }
        // Protocol does not exist, try to create it
        catch (EntityNotFoundException)
        {
            await _authorizationService.CheckPermissions(loan,
                LoanOperations.CreateReturnProtocol);
            var protocol = await _returnProtocolFacade.CreateReturnProtocol(loan, request);

            // generate response
            var response = await _responseGenerator.GenerateReturnProtocolDetailResponse(protocol);

            return CreatedAtAction(_urlHelper.GetUriByAction(HttpContext, nameof(GetProtocol), values: protocol.Id),
                response);
        }
    }

    /// <summary>
    /// Returns all images of the given return protocol.
    /// </summary>
    /// <param name="loanId">Loan loanId.</param>
    /// <returns>All images associated with the item.</returns>
    [HttpGet("images")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetImages(int loanId)
    {
        // get the item and check permissions
        var loan = await _loanFacade.GetLoan(loanId);

        // Check permission for the loan.
        await _authorizationService.CheckPermissions(loan, LoanOperations.Read);

        // get return protocol instance
        var returnProtocol = _returnProtocolFacade.GetReturnProtocol(loan);

        // Check permissions for the return protocol
        await _authorizationService.CheckPermissions(returnProtocol,
            ReturnProtocolOperations.Read);

        // get the images and map them to response
        var images = returnProtocol.Images;

        // generate response 
        var response = await _imageResponseGenerator.GenerateResponseList(images);

        return Ok(response);
    }

    /// <summary>
    /// Creates new image for the given return protocol.
    /// </summary>
    /// <param name="loanId">Item loanId.</param>
    /// <param name="file">Image file.</param>
    /// <returns>Newly create image.</returns>
    /// <response code="201">Returns newly created image.</response>
    /// <response code="400">If the request is not valid.</response>
    /// <response code="403">If the user is not authorized to create the image.</response>
    [HttpPost("images")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> AddImage(int loanId, IFormFile file)
    {
        // get the item and check permissions
        var loan = await _loanFacade.GetLoan(loanId);

        // Verify that the user can read the loan data
        await _authorizationService.CheckPermissions(loan, LoanOperations.Read);

        // get return protocol instance
        var returnProtocol = _returnProtocolFacade.GetReturnProtocol(loan);

        // Check permissions for creating images of the return protocol
        await _authorizationService.CheckPermissions(returnProtocol, ReturnProtocolOperations.Update);

        // Save the image to the file system
        var filePath = await _fileUploadService.SaveUploadedImage(file);

        // Create new image
        var image = new Image()
        {
            Name = file.FileName,
            Extension = _fileUploadService.GetFileExtension(file),
            MimeType = _fileUploadService.GetMimeType(file),
            ReturnProtocol = returnProtocol
        };

        // Save the image to the database
        await _returnProtocolFacade.AddImage(returnProtocol, image, filePath);

        // Delete temporary file
        _fileUploadService.CleanUp(filePath);

        // Map the image to response
        var response = await _imageResponseGenerator.GenerateImageDetailResponse(image);

        return Created(_imageResponseGenerator.GetLink(image), response);
    }

    /// <summary>
    /// Deleted given image of the return protocol.
    /// </summary>
    /// <param name="loanId">Item loanId.</param>
    /// <param name="imageId">Image id.</param>
    /// <returns></returns>
    /// <response code="204">Image has been deleted</response>
    /// <response code="400">If the request is not valid.</response>
    /// <response code="403">If the user is not authorized to delete the image.</response>
    /// <response code="404">If the image does not exist.</response>
    [HttpDelete("images/{imageId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteImage(int loanId, int imageId)
    {
        // get the loan and check permissions
        var loan = await _loanFacade.GetLoan(loanId);

        // Verify that the user can read the loan data
        await _authorizationService.CheckPermissions(loan, LoanOperations.Read);

        // get return protocol instance
        var returnProtocol = _returnProtocolFacade.GetReturnProtocol(loan);

        // Check permissions for updating the return protocol
        await _authorizationService.CheckPermissions(returnProtocol, ReturnProtocolOperations.Update);

        // Get the image
        var image = await _returnProtocolFacade.GetImage(returnProtocol.Id, imageId);

        // Delete the image
        await _returnProtocolFacade.DeleteImage(image);

        return NoContent();
    }
}
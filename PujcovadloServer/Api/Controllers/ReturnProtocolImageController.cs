using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PujcovadloServer.Api.Filters;
using PujcovadloServer.Api.Services;
using PujcovadloServer.AuthorizationHandlers;
using PujcovadloServer.Business.Entities;
using PujcovadloServer.Business.Facades;

namespace PujcovadloServer.Api.Controllers;

[ApiController]
[Route("api/loans/{loanId}/return-protocol/images")]
[ServiceFilter(typeof(ExceptionFilter))]
public class ReturnProtocolImageController : ACrudController<Image>
{
    private readonly LoanFacade _loanFacade;
    private readonly ReturnProtocolFacade _returnProtocolFacade;
    private readonly ImageResponseGenerator _responseGenerator;
    private readonly IMapper _mapper;
    private readonly FileUploadService _fileUploadService;

    public ReturnProtocolImageController(LoanFacade loanFacade, ReturnProtocolFacade returnProtocolFacade,
        ImageResponseGenerator responseGenerator, IMapper mapper, AuthorizationService authorizationService,
        LinkGenerator urlHelper, FileUploadService fileUploadService) : base(authorizationService, urlHelper)
    {
        _loanFacade = loanFacade;
        _returnProtocolFacade = returnProtocolFacade;
        _responseGenerator = responseGenerator;
        _mapper = mapper;
        _fileUploadService = fileUploadService;
    }

    /// <summary>
    /// Returns all images of the given return protocol.
    /// </summary>
    /// <param name="loanId">Loan loanId.</param>
    /// <returns>All images associated with the item.</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [AllowAnonymous]
    public async Task<IActionResult> GetImages(int loanId)
    {
        // get the item and check permissions
        var loan = await _loanFacade.GetLoan(loanId);

        // Check permission for the item.
        await _authorizationService.CheckPermissions(loan, ItemAuthorizationHandler.Operations.Read);

        // get return protocol instance
        var returnProtocol = _returnProtocolFacade.GetReturnProtocol(loan);

        // Check permissions for the return protocol
        await _authorizationService.CheckPermissions(returnProtocol,
            ReturnProtocolAuthorizationHandler.Operations.Read);

        // get the images and map them to response
        var images = returnProtocol.Images;

        // generate response 
        var response = await _responseGenerator.GenerateResponseList(images);

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
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> AddImage(int loanId, IFormFile file)
    {
        // get the item and check permissions
        var loan = await _loanFacade.GetLoan(loanId);

        // Verify that the user can read the loan data
        await _authorizationService.CheckPermissions(loan, LoanAuthorizationHandler.Operations.Read);

        // get return protocol instance
        var returnProtocol = _returnProtocolFacade.GetReturnProtocol(loan);

        // Check permissions for creating images of the return protocol
        await _authorizationService.CheckPermissions(returnProtocol,
            ReturnProtocolAuthorizationHandler.Operations.Update);

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

        await _authorizationService.CheckPermissions(image, ImageAuthorizationHandler.Operations.Create);

        // Save the image to the database
        await _returnProtocolFacade.AddReturnProtocolImage(returnProtocol, image, filePath);

        // Map the image to response
        var response = await _responseGenerator.GenerateImageDetailResponse(image);

        return Created(_responseGenerator.GetLink(image), response);
    }
}
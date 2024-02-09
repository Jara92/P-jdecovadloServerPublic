using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PujcovadloServer.Api.Filters;
using PujcovadloServer.Api.Services;
using PujcovadloServer.AuthorizationHandlers;
using PujcovadloServer.Business.Entities;
using PujcovadloServer.Business.Facades;
using PujcovadloServer.Responses;

namespace PujcovadloServer.Api.Controllers;

[ApiController]
[Route("api/items")]
[ServiceFilter(typeof(ExceptionFilter))]
public class ItemImageController : ACrudController<Image>
{
    private readonly ItemFacade _itemFacade;
    private readonly ImageFacade _imageFacade;
    private readonly ImageResponseGenerator _responseGenerator;
    private readonly IMapper _mapper;
    private readonly FileUploadService _fileUploadService;

    public ItemImageController(ItemFacade itemFacade, ImageFacade imageFacade, ImageResponseGenerator responseGenerator, IMapper mapper,
        AuthorizationService authorizationService, LinkGenerator urlHelper,
        FileUploadService fileUploadService) : base(authorizationService, urlHelper)
    {
        _itemFacade = itemFacade;
        _imageFacade = imageFacade;
        _responseGenerator = responseGenerator;
        _mapper = mapper;
        _fileUploadService = fileUploadService;
    }

    /// <summary>
    /// Returns all images of the given item.
    /// </summary>
    /// <param name="id">Item id.</param>
    /// <returns>All images associated with the item.</returns>
    [HttpGet("{id}/images")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [AllowAnonymous]
    public async Task<IActionResult> GetImages(int id)
    {
        // get the item and check permissions
        var item = await _itemFacade.GetItem(id);

        // Check permission for the item.
        await _authorizationService.CheckPermissions(item, ItemAuthorizationHandler.Operations.Read);

        // get the images and map them to response
        var images = item.Images;
        
        // generate response 
        var response = await _responseGenerator.GenerateResponseList(images);

        return Ok(response);
    }

    /// <summary>
    /// Returns image by given id.
    /// </summary>
    /// <param name="id">Item id.</param>
    /// <param name="imageId">Image id.</param>
    /// <returns>Returns image file.</returns>
    /// <response code="200">Returns image file.</response>
    /// <response code="400">If the request is not valid.</response>
    /// <response code="403">If the user is not authorized to read the image.</response>
    /// <response code="404">If the image was not found.</response>
    [HttpGet("{id}/images/{imageId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ImageResponse>> GetImage(int id, int imageId)
    {
        // get the image and return it
        var image = await _imageFacade.GetImage(id, imageId);
        await _authorizationService.CheckPermissions(image, ItemAuthorizationHandler.Operations.Read);

        // Generate response
        var response = await _responseGenerator.GenerateImageDetailResponse(image);

        return Ok(response);
    }

    /// <summary>
    /// Creates new image for the given item.
    /// </summary>
    /// <param name="id">Item id.</param>
    /// <param name="file">Image file.</param>
    /// <returns>Newly create image.</returns>
    /// <response code="201">Returns newly created image.</response>
    /// <response code="400">If the request is not valid.</response>
    /// <response code="403">If the user is not authorized to create the image.</response>
    [HttpPost("{id}/images")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> AddImage(int id, IFormFile file)
    {
        // get the item and check permissions
        var item = await _itemFacade.GetItem(id);

        // Save the image to the file system
        var filePath = await _fileUploadService.SaveUploadedImage(file);

        // Create new image
        var image = new Image()
        {
            Name = file.FileName,
            Extension = _fileUploadService.GetFileExtension(file),
            MimeType = _fileUploadService.GetMimeType(file),
            Item = item
        };

        await _authorizationService.CheckPermissions(image, ItemAuthorizationHandler.Operations.Create);

        // Save the image to the database
        await _itemFacade.AddImage(item, image, filePath);

        // Map the image to response
        var response = await _responseGenerator.GenerateImageDetailResponse(image);

        return Created(_urlHelper.GetUriByAction(HttpContext, nameof(GetImage),
            values: new { id = item.Id, imageId = image.Id }), response);
    }

    /// <summary>
    /// Deletes image by given id.
    /// </summary>
    /// <param name="id">item id</param>
    /// <param name="imageId">Image id.</param>
    /// <returns></returns>
    /// <response code="204">If the image was deleted successfully.</response>
    /// <response code="400">If the request is not valid.</response>
    /// <response code="403">If the user is not authorized to delete the image.</response>
    /// <response code="404">If the image was not found.</response>
    [HttpDelete("{id}/images/{imageId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteImage(int id, int imageId)
    {
        // get the image
        var image = await _imageFacade.GetImage(id, imageId);
        await _authorizationService.CheckPermissions(image, ItemAuthorizationHandler.Operations.Delete);

        // get the image and return it
        await _imageFacade.DeleteImage(id, imageId);

        return NoContent();
    }
}
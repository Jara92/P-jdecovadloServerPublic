using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PujcovadloServer.Api.Filters;
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
    private readonly IMapper _mapper;

    public ItemImageController(ItemFacade itemFacade, ImageFacade imageFacade, IMapper mapper,
        IAuthorizationService authorizationService, LinkGenerator urlHelper) : base(authorizationService, urlHelper)
    {
        _itemFacade = itemFacade;
        _imageFacade = imageFacade;
        _mapper = mapper;
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
        await CheckPermissions<Item>(item, ItemAuthorizationHandler.Operations.Read);

        // get the images and map them to response
        var images = item.Images;
        var responseImages = _mapper.Map<List<ImageResponse>>(images);

        // Hateos image links
        foreach (var image in responseImages)
        {
            image.Links.Add(new LinkResponse(
                _urlHelper.GetUriByAction(HttpContext, nameof(GetImage), "ItemImage",
                    values: new { id, imageId = image.Id }), "SELF", "GET"));
        }
        
        // Create response with links
        var response = new ResponseList<ImageResponse>()
        {
            Data = responseImages,
            Links = new List<LinkResponse>
            {
                new LinkResponse(_urlHelper.GetUriByAction(HttpContext, nameof(ItemController.Get), "Item", values: new { id }), "ITEM", "GET")
            }
        };

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
    [AllowAnonymous]
    public async Task<IActionResult> GetImage(int id, int imageId)
    {
        // get the item and check permissions
        var item = await _itemFacade.GetItem(id);

        // get the image and return it
        var image = await _imageFacade.GetImage(item, imageId);
        await CheckPermissions(image, ItemAuthorizationHandler.Operations.Read);

        if (System.IO.File.Exists(image.Path))
        {
            // Get all bytes of the file and return the file with the specified file contents 
            byte[] b = await System.IO.File.ReadAllBytesAsync(image.Path);
            return File(b, "application/octet-stream", "soubor.png");
        }

        // return error if file not found
        return StatusCode(StatusCodes.Status404NotFound);
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

        // TODO: make path and max file size configurable
        var filePath = Path.GetTempFileName();
        // var filePath = Path.Combine(_config["StoredFilesPath"], Path.GetRandomFileName());

        // Save the file as temporary file
        using (var stream = System.IO.File.Create(filePath))
        {
            await file.CopyToAsync(stream);
        }

        var image = new Image()
        {
            Name = file.FileName,
            Path = filePath,
            Item = item
        };
        
        await CheckPermissions(image, ItemAuthorizationHandler.Operations.Create);

        // Save the image to the database
        await _imageFacade.Create(image);
        var imageResponse = _mapper.Map<ImageResponse>(image);

        return Created(_urlHelper.GetUriByAction(HttpContext, nameof(GetImage),
            values: new { id = item.Id, imageId = image.Id }), imageResponse);
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
        // get the item and check permissions
        var item = await _itemFacade.GetItem(id);
        
        // get the image
        var image = await _imageFacade.GetImage(item, imageId);
        await CheckPermissions(image, ItemAuthorizationHandler.Operations.Delete);

        // get the image and return it
        await _imageFacade.DeleteImage(item, imageId);
        
        return NoContent();
    }
}
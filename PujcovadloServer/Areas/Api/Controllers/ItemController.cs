using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PujcovadloServer.Areas.Api.Filters;
using PujcovadloServer.Areas.Api.Services;
using PujcovadloServer.AuthorizationHandlers;
using PujcovadloServer.AuthorizationHandlers.Item;
using PujcovadloServer.Business.Entities;
using PujcovadloServer.Business.Enums;
using PujcovadloServer.Business.Facades;
using PujcovadloServer.Business.Filters;
using PujcovadloServer.Requests;
using PujcovadloServer.Responses;

namespace PujcovadloServer.Areas.Api.Controllers;

[Area("Api")]
[ApiController]
[Route("api/items")]
[ServiceFilter(typeof(ExceptionFilter))]
public class ItemController : ACrudController<Item>
{
    private readonly IMapper _mapper;
    private readonly ItemFacade _itemFacade;
    private readonly ItemResponseGenerator _itemResponseGenerator;
    private readonly ImageResponseGenerator _imageResponseGenerator;
    private readonly FileUploadService _fileUploadService;
    private readonly AuthorizationService _authorizationService;

    public ItemController(ItemFacade itemFacade, IMapper mapper, LinkGenerator urlHelper,
        ItemResponseGenerator itemResponseGenerator, ImageResponseGenerator imageResponseGenerator,
        FileUploadService fileUploadService, AuthorizationService authorizationService) : base(
        authorizationService, urlHelper)
    {
        _itemFacade = itemFacade;
        _mapper = mapper;
        _itemResponseGenerator = itemResponseGenerator;
        _imageResponseGenerator = imageResponseGenerator;
        _fileUploadService = fileUploadService;
        _authorizationService = authorizationService;
    }

    /// <summary>
    /// Returns all items by given filter.
    /// </summary>
    /// <param name="filter">Filtering object</param>
    /// <returns>Paginated, filtered and sorted items.</returns>
    /// <response code="200">Returns paginated list of items.</response>
    /// <response code="400">If filter input is invalid.</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<List<ItemResponse>>> Index([FromQuery] ItemFilter filter)
    {
        // Get items
        var items = await _itemFacade.GetAll(filter);

        // get response list
        var response = await _itemResponseGenerator.GenerateResponseList(items, filter, nameof(Index), "Item");

        return Ok(response);
    }

    /// <summary>
    /// Returns item with given id.
    /// </summary>
    /// <param name="id">Item's id.</param>
    /// <returns>Item identified by the id.</returns>
    /// <response code="200">Returns item with given id.</response>
    /// <response code="404">If item with given id was not found.</response>
    [HttpGet("{id}", Name = nameof(Get))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ItemDetailResponse>> Get(int id)
    {
        // Get the item and convert it to response
        var item = await _itemFacade.GetItem(id);

        await _authorizationService.CheckPermissions(item, ItemOperations.Read);

        // get item response
        var responseItem = await _itemResponseGenerator.GenerateItemDetailResponse(item);

        return Ok(responseItem);
    }

    /// <summary>
    /// Create a new item.
    /// </summary>
    /// <param name="request">New Item</param>
    /// <returns>Newly created item.</returns>
    /// <response code="201">Returns the newly created item.</response>
    /// <response code="400">If the item is invalid.</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [Authorize(Roles = UserRoles.Owner)]
    public async Task<ActionResult<ItemResponse>> Create([FromBody] ItemRequest request)
    {
        await _authorizationService.CheckPermissions(_mapper.Map<Item>(request),
            ItemOperations.Create);

        var newItem = await _itemFacade.CreateItem(request);

        // generate detailed reponse for the owner
        var responseItem = await _itemResponseGenerator.GenerateItemOwnerResponse(newItem);

        // generate response with location header
        return CreatedAtAction(_urlHelper.GetUriByAction(HttpContext, nameof(Get), values: newItem.Id), responseItem);
    }

    /// <summary>
    /// Updates item with given id.
    /// </summary>
    /// <param name="id">Item's id.</param>
    /// <param name="request">Updated item.</param>
    /// <returns></returns>
    /// <response code="204">If the item was updated successfully.</response>
    /// <response code="400">If the item data is invalid.</response>
    /// <response code="404">If the item was not found.</response>
    /// <response code="409">If the item was updated in the meantime.</response>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ValidateIdFilter]
    [Authorize(Roles = UserRoles.Owner)]
    public async Task<IActionResult> Update(int id, [FromBody] ItemRequest request)
    {
        var item = await _itemFacade.GetItem(id);

        await _authorizationService.CheckPermissions(item, ItemOperations.Update);

        // Update the item
        await _itemFacade.UpdateItem(item, request);

        return NoContent();
    }

    /// <summary>
    /// Deletes item with given id.
    /// </summary>
    /// <param name="id">item's id</param>
    /// <returns></returns>
    /// <response code="204">If the item was deleted successfully.</response>
    /// <response code="404">If the item was not found.</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        // Get the item
        var item = await _itemFacade.GetItem(id);

        await _authorizationService.CheckPermissions(item, ItemOperations.Delete);

        await _itemFacade.DeleteItem(item);

        return NoContent();
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
    public async Task<IActionResult> GetImages(int id)
    {
        // get the item and check permissions
        var item = await _itemFacade.GetItem(id);

        // Check permission for the item.
        await _authorizationService.CheckPermissions(item, ItemOperations.Read);

        // get the images and map them to response
        var images = item.Images;

        // generate response 
        var response = await _imageResponseGenerator.GenerateResponseList(images);

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

        await _authorizationService.CheckPermissions(item, ItemOperations.Update);

        // Save the image to the database
        await _itemFacade.AddImage(item, image, filePath);

        // Delete temporary file
        _fileUploadService.CleanUp(filePath);

        // Map the image to response
        var response = await _imageResponseGenerator.GenerateImageDetailResponse(image);

        return Created(_imageResponseGenerator.GetLink(image), response);
    }

    /// <summary>
    /// Deletes given image of the item.
    /// </summary>
    /// <param name="itemId">Item id.</param>
    /// <param name="imageId">Image id.</param>
    /// <returns></returns>
    /// <response code="204">Image has been deleted</response>
    /// <response code="400">If the request is not valid.</response>
    /// <response code="403">If the user is not authorized to delete the image.</response>
    /// <response code="404">If the image does not exist.</response>
    [HttpDelete("{itemId}/images/{imageId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteImage(int itemId, int imageId)
    {
        // get the loan and check permissions
        var item = await _itemFacade.GetItem(itemId);

        // Verify that the user can read the loan data
        await _authorizationService.CheckPermissions(item, ItemOperations.Update);

        // Get the image
        var image = await _itemFacade.GetImage(item.Id, imageId);

        // Delete the image
        await _itemFacade.DeleteImage(image);

        return NoContent();
    }
}
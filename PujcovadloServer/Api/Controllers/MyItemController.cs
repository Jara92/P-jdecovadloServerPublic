using System.Security.Claims;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PujcovadloServer.Api.Filters;
using PujcovadloServer.Authentication;
using PujcovadloServer.AuthorizationHandlers;
using PujcovadloServer.Business.Entities;
using PujcovadloServer.Business.Enums;
using PujcovadloServer.Business.Facades;
using PujcovadloServer.Business.Filters;
using PujcovadloServer.Business.Services;
using PujcovadloServer.Requests;
using PujcovadloServer.Responses;

namespace PujcovadloServer.Api.Controllers;

[ApiController]
[Route("api/my-items")]
[Authorize(Roles = UserRoles.Owner)]
[ServiceFilter(typeof(ExceptionFilter))]
public class MyItemController : ACrudController<Item>
{
    private readonly ItemService _itemService;
    private readonly ItemFacade _itemFacade;
    private readonly ImageFacade _imageFacade;
    private readonly IMapper _mapper;

    public MyItemController(ItemFacade itemFacade, ImageFacade imageFacade, ItemService itemService,
        LinkGenerator urlHelper, IMapper mapper, IAuthorizationService authorizationService) : base(authorizationService, urlHelper)
    {
        _itemService = itemService;
        _itemFacade = itemFacade;
        _imageFacade = imageFacade;
        _mapper = mapper;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ItemOwnerResponse>> Index([FromQuery] ItemFilter filter)
    {
        // Get items
        var items = await _itemFacade.GetMyItems(filter);

        // Map items to response
        var responseItems = _mapper.Map<List<ItemOwnerResponse>>(items);

        // Hateos item links
        foreach (var item in responseItems)
        {
            item.Links.Add(new LinkResponse(
                _urlHelper.GetUriByAction(HttpContext, nameof(ItemController.Get), "Item", values: new { item.Id }),
                "SELF", "GET"));
            item.Links.Add(new LinkResponse(
                _urlHelper.GetUriByAction(HttpContext, nameof(MyItemController.Update), "MyItem",
                    values: new { item.Id }),
                "UPDATE", "PUT"));
            item.Links.Add(new LinkResponse(
                _urlHelper.GetUriByAction(HttpContext, nameof(MyItemController.Delete), "MyItem",
                    values: new { item.Id }),
                "DELETE", "DELETE"));
        }

        // Generate pagination links
        var links = GeneratePaginationLinks(items, filter, nameof(Index));

        // Return response
        return Ok(new ResponseList<ItemOwnerResponse>
        {
            Data = responseItems,
            Links = links
        });
    }

    /// <summary>
    /// Returns full item by given id.
    /// </summary>
    /// <param name="id">Id of the item</param>
    /// <returns>Returns all information about the item.</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ItemOwnerResponse>> Get(int id)
    {
        var item = await _itemFacade.GetItem(id);

        // Can read all item's data only if the user is can update the item
        await CheckPermissions(item, ItemAuthorizationHandler.Operations.Update);

        var response = _mapper.Map<ItemOwnerResponse>(item);

        response.Links.Add(new LinkResponse(
            _urlHelper.GetUriByAction(HttpContext, nameof(MyItemController.Get), "MyItem", values: new { response.Id }),
            "SELF", "GET"));

        return Ok(response);
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
        await CheckPermissions(_mapper.Map<Item>(request), ItemAuthorizationHandler.Operations.Create);

        var newItem = await _itemFacade.CreateItem(request);

        var responseItem = _mapper.Map<ItemResponse>(newItem);

        // generate response with location header
        return CreatedAtAction(_urlHelper.GetUriByAction(HttpContext, nameof(Get), values: newItem.Id), responseItem);
    }

    /// <summary>
    /// Updates item with given id.
    /// </summary>
    /// <param name="id">Item's id.</param>
    /// <param name="request">Updated item.</param>
    /// <returns></returns>
    /// <response code="200">If the item was updated successfully.</response>
    /// <response code="400">If the item data is invalid.</response>
    /// <response code="404">If the item was not found.</response>
    /// <response code="409">If the item was updated in the meantime.</response>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ValidateIdFilter]
    [Authorize(Roles = UserRoles.Owner)]
    public async Task<IActionResult> Update(int id, [FromBody] ItemRequest request)
    {
        var item = await _itemFacade.GetItem(id);

        await CheckPermissions(item, ItemAuthorizationHandler.Operations.Update);

        // Update the item
        await _itemFacade.UpdateItem(item, request);

        return Ok();
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

        await CheckPermissions(item, ItemAuthorizationHandler.Operations.Delete);

        await _itemFacade.DeleteItem(item);

        return NoContent();
    }

    [HttpGet("{id}/images")]
    public async Task<IActionResult> GetImages(int id)
    {
        // get the item and check permissions
        var item = await _itemFacade.GetItem(id);
        await CheckPermissions(item, ItemAuthorizationHandler.Operations.Read);

        // get the images and map them to response
        var images = item.Images;
        var responseImages = _mapper.Map<List<ImageResponse>>(images);

        return Ok(responseImages);
    }
    
    [HttpGet("{id}/images/{imageId}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetImage(int id, int imageId)
    {
        // get the item and check permissions
        var item = await _itemFacade.GetItem(id);
        await CheckPermissions(item, ItemAuthorizationHandler.Operations.Read);

        // get the image and return it
        var image = await _imageFacade.GetImage(imageId);
        
        if (System.IO.File.Exists(image.Path))
        {
            // Get all bytes of the file and return the file with the specified file contents 
            byte[] b = await System.IO.File.ReadAllBytesAsync(image.Path);
            return File(b, "application/octet-stream", "soubor.png");
        } 

        else
        {
            // return error if file not found
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
        
        // TODO: TEST this
        
        // TODO: return the image file not the resource
        //return Ok(_mapper.Map<ImageResponse>(image));
    }

    [HttpPost("{id}/images")]
    public async Task<IActionResult> AddImage(int id, IFormFile file)
    {
        // get the item and check permissions
        var item = await _itemFacade.GetItem(id);
        await CheckPermissions(item, ItemAuthorizationHandler.Operations.Update);

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
            Name = file.Name,
            Path = filePath,
            Item = item
        };

        // Save the image to the database
        await _imageFacade.Create(image);
        var imageResponse = _mapper.Map<ImageResponse>(image);
        
        return Created(_urlHelper.GetUriByAction(HttpContext, nameof(GetImage), "MyItem",
            values: new { id = item.Id, imageId = image.Id }), imageResponse);
    }
}
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PujcovadloServer.Business.Entities;
using PujcovadloServer.Business.Exceptions;
using PujcovadloServer.Business.Facades;
using PujcovadloServer.Business.Filters;
using PujcovadloServer.Business.Interfaces;
using PujcovadloServer.Business.Services;
using PujcovadloServer.Requests;
using PujcovadloServer.Responses;

namespace PujcovadloServer.Api.Controllers;

[ApiController]
[Route("api/items")]
public class ItemController : ACrudController
{
    private readonly IMapper _mapper;
    private readonly IItemRepository _itemsRepository;
    private readonly ItemService _itemService;
    private readonly ItemFacade _itemFacade;
    private readonly ItemCategoryFacade _itemCategoryFacade;
    private readonly LinkGenerator _urlHelper;

    public ItemController(IItemRepository itemsRepository, ItemFacade itemFacade,
        ItemCategoryFacade itemCategoryFacade, IMapper mapper,
        ItemService itemService, LinkGenerator urlHelper) : base(urlHelper)
    {
        _itemsRepository = itemsRepository;
        _itemFacade = itemFacade;
        _itemCategoryFacade = itemCategoryFacade;
        _mapper = mapper;
        _itemService = itemService;
        _urlHelper = urlHelper;
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
        var items = await _itemService.GetAll(filter);

        // Map items to response
        var responseItems = _mapper.Map<List<ItemResponse>>(items);

        // Hateos item links
        foreach (var item in responseItems)
        {
            item.Links.Add(new LinkResponse(
                _urlHelper.GetUriByAction(HttpContext, nameof(this.Get), values: new { item.Id }), "SELF", "GET"));
        }

        // Hateos links
        var links = GeneratePaginationLinks(items, filter, nameof(Index));

        // Build response
        var response = new ResponseList<ItemResponse>
        {
            Data = responseItems,
            Links = links
        };


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
        var item = await _itemService.Get(id);

        if (item == null)
            return NotFound($"Item with {id} not found.");

        var responseItem = _mapper.Map<ItemDetailResponse>(item);

        // Hateos links
        responseItem.Links.Add(new LinkResponse(
            _urlHelper.GetUriByAction(HttpContext, nameof(this.Index)), "LIST", "GET"));

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
    public async Task<ActionResult<ItemResponse>> Create([FromBody] ItemRequest request)
    {
        var newItem = await _itemFacade.CreateItem(request);
        var responseItem = _mapper.Map<ItemResponse>(newItem);

        // generate response with location header
        return Created(_urlHelper.GetUriByAction(HttpContext, nameof(Get), values: newItem.Id), responseItem);
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
    public async Task<IActionResult> Update(int id, [FromBody] ItemRequest request)
    {
        // Check if item with given id exists
        if (request.Id != id)
        {
            return BadRequest("Id in url and body must be the same.");
        }

        try
        {
            await _itemFacade.UpdateItem(request);
        }
        catch (EntityNotFoundException)
        {
            return NotFound($"Item with {id} was not found.");
        }
        catch (DbUpdateConcurrencyException)
        {
            return Conflict($"Item with {id} was updated in the meantime. Try again.");
        }

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
        try
        {
            await _itemFacade.DeleteItem(id);
        }
        catch (EntityNotFoundException ex)
        {
            return NotFound(ex.Message);
        }

        return NoContent();
    }

    /// <summary>
    /// Returns item's categories.
    /// </summary>
    /// <param name="id">Item's id</param>
    /// <returns>All categories related to the item.</returns>
    /// <response code="200">Returns all categories related to the item.</response>
    /// <response code="404">If the item was not found.</response>
    [HttpGet("{id}/categories")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<ItemCategory>>> GetCategories(int id)
    {
        var item = await _itemsRepository.Get(id);

        if (item == null)
            return NotFound($"Item with {id} not found.");

        return Ok(item.Categories);
    }
}
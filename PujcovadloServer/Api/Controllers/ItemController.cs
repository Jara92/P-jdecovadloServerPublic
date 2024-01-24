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
public class ItemController : ControllerBase
{
    private readonly IMapper _mapper;
    private readonly IItemRepository _itemsRepository;
    private readonly ItemService _itemService;
    private readonly ItemFacade _itemFacade;
    private readonly ItemCategoriesFacade _itemCategoriesFacade;
    private readonly LinkGenerator _urlHelper;

    public ItemController(IItemRepository itemsRepository, ItemFacade itemFacade,
        ItemCategoriesFacade itemCategoriesFacade, IMapper mapper,
        ItemService itemService, LinkGenerator urlHelper)
    {
        _itemsRepository = itemsRepository;
        _itemFacade = itemFacade;
        _itemCategoriesFacade = itemCategoriesFacade;
        _mapper = mapper;
        _itemService = itemService;
        _urlHelper = urlHelper;
    }

    /**
     * GET /api/items
     *
     * Returns all items.
     */
    [HttpGet]
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
        var links = new List<LinkResponse>();

        // Next page link
        if (items.HasNextPage)
        {
            var nextPageFilter = new ItemFilter(filter)
            {
                Page = items.PageIndex + 1
            };

            links.Add(new LinkResponse(
                _urlHelper.GetUriByAction(HttpContext, nameof(this.Index), values: nextPageFilter), "NEXT", "GET"));
        }

        // Previous page link
        if (items.HasPreviousPage)
        {
            var previousPageFilter = new ItemFilter(filter)
            {
                Page = items.PageIndex - 1
            };
            
            links.Add(new LinkResponse(
                _urlHelper.GetUriByAction(HttpContext, nameof(this.Index), values: previousPageFilter), "PREVIOUS", "GET"));
        }

        // Build response
        var response = new ResponseList()
        {
            Data = responseItems,
            Links = links
        };
        

        return Ok(response);
    }

    /**
     * GET /api/items/{id}
     *
     * Returns item with given id.
     */
    [HttpGet("{id}", Name = nameof(Get))]
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

    /**
     * POST /api/items
     *
     * Creates new item.
     */
    [HttpPost]
    public async Task<ActionResult<ItemResponse>> Create([FromBody] ItemRequest request)
    {
        var newItem = await _itemFacade.CreateItem(request);

        // generate response with location header
        return Created($"/api/items/{newItem.Id}", _mapper.Map<ItemResponse>(newItem));
    }

    /**
     * PUT /api/items/{id}
     *
     * Updates item with given id.
     */
    [HttpPut("{id}")]
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

    /**
     * DELETE /api/items/{id}
     *
     * Deletes item with given id.
     */
    [HttpDelete("{id}")]
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

    [HttpGet("{id}/categories")]
    public async Task<ActionResult<List<ItemCategory>>> GetCategories(int id)
    {
        var item = await _itemsRepository.Get(id);

        if (item == null)
            return NotFound($"Item with {id} not found.");

        return Ok(item.Categories);
    }

    // TODO REMOVE THIS FUCKING PIECE OF SHIT
    [HttpPost("{id}/categories")]
    public async Task<ActionResult<ItemCategory>> AddCategory(int id, [FromBody] ItemCategory category)
    {
        var item = _itemsRepository.Get(id).Result;

        if (item == null)
            return NotFound($"Item with {id} not found.");

        var dbCategory = await _itemCategoriesFacade.Get(category.Id);

        if (dbCategory == null)
            return NotFound($"ItemCategory with {category.Id} not found.");

        // Add category to item
        await _itemFacade.AddCategory(item, dbCategory);

        return Created($"/api/items/{id}/categories/{dbCategory.Id}", null);
    }
}
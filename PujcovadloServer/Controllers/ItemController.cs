using System.ComponentModel;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PujcovadloServer.Exceptions;
using PujcovadloServer.Facades;
using PujcovadloServer.Models;
using PujcovadloServer.Requests;
using PujcovadloServer.Responses;
using PujcovadloServer.Repositories;
using PujcovadloServer.Repositories.Interfaces;
using PujcovadloServer.Services;
using PujcovadloServer.Services.Interfaces;

namespace PujcovadloServer.Controllers;

[ApiController]
[Route("api/items")]
public class ItemController : ControllerBase
{
    private readonly IMapper _mapper;
    private readonly IItemRepository _itemsRepository;
    private readonly ItemService _itemService;
    private readonly ItemFacade _itemFacade;
    private readonly ItemCategoriesFacade _itemCategoriesFacade;

    public ItemController(IItemRepository itemsRepository, ItemFacade itemFacade,
        ItemCategoriesFacade itemCategoriesFacade, IMapper mapper,
        ItemService itemService)
    {
        _itemsRepository = itemsRepository;
        _itemFacade = itemFacade;
        _itemCategoriesFacade = itemCategoriesFacade;
        _mapper = mapper;
        _itemService = itemService;
    }

    /**
     * GET /api/items
     *
     * Returns all items.
     */
    [HttpGet]
    public async Task<ActionResult<List<ItemResponse>>> Index()
    {
        var items = await _itemService.GetAll();
        var responseItems = _mapper.Map<List<ItemResponse>>(items);
        
        return Ok(responseItems);
    }

    /**
     * GET /api/items/{id}
     *
     * Returns item with given id.
     */
    [HttpGet("{id}")]
    public async Task<ActionResult<ItemResponse>> Get(int id)
    {
        var item = await _itemsRepository.Get(id);

        if (item == null)
        {
            return NotFound($"Item with {id} not found.");
        }
        
        var responseItem = _mapper.Map<ItemResponse>(item);

        return Ok(responseItem);
    }

    /**
     * POST /api/items
     *
     * Creates new item.
     */
    [HttpPost]
    public async Task<ActionResult<ItemResponse>> Create([FromBody] ItemRequest itemDto)
    {
        var newItem = _mapper.Map<Item>(itemDto);
        
        await _itemFacade.CreateItem(newItem);

        // generate response with location header
        var response = Created($"/api/items/{newItem.Id}", _mapper.Map<ItemResponse>(newItem));

        return response;
    }

    /**
     * PUT /api/items/{id}
     *
     * Updates item with given id.
     */
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] ItemRequest itemDto)
    {
        // Check if item with given id exists
        if (itemDto.Id != id)
        {
            return BadRequest("Id in url and body must be the same.");
        }
        
        // Get old item
        var oldItem = await _itemService.Get(id);
        if (oldItem == null)
        {
            return NotFound($"Item with {id} not found.");
        }

        try
        {
            await _itemFacade.UpdateItem(oldItem, itemDto);
        }
        catch(DbUpdateConcurrencyException)
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
        var dbItem = await _itemService.Get(id);

        if (dbItem == null)
            return NotFound($"Item with {id} not found.");

        await _itemService.Delete(dbItem);

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
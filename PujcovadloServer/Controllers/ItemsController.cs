using System.ComponentModel;
using Microsoft.AspNetCore.Mvc;
using PujcovadloServer.Facades;
using PujcovadloServer.Models;
using PujcovadloServer.Repositories;

namespace PujcovadloServer.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ItemsController : ControllerBase
{
    private readonly ItemsRepository _itemsRepository;
    private readonly ItemsFacade _itemsFacade;
    private readonly ItemCategoriesFacade _itemCategoriesFacade;

    public ItemsController(ItemsRepository itemsRepository, ItemsFacade itemsFacade,
        ItemCategoriesFacade itemCategoriesFacade)
    {
        _itemsRepository = itemsRepository;
        _itemsFacade = itemsFacade;
        _itemCategoriesFacade = itemCategoriesFacade;
    }

    /**
     * GET /api/items
     *
     * Returns all items.
     */
    [HttpGet]
    public async Task<ActionResult<List<Item>>> Index()
    {
        var items = await _itemsRepository.GetAll();

        return Ok(items);
    }

    /**
     * GET /api/items/{id}
     *
     * Returns item with given id.
     */
    [HttpGet("{id}")]
    public async Task<ActionResult<Item>> Get(int id)
    {
        var item = await _itemsRepository.Get(id);

        if (item == null)
        {
            return NotFound($"Item with {id} not found.");
        }

        return Ok(item);
    }

    /**
     * POST /api/items
     *
     * Creates new item.
     */
    [HttpPost]
    public async Task<ActionResult<Item>> Create([FromBody] Item item)
    {
        var newItem = new Item();
        FillNewData(newItem, item);

        await _itemsFacade.CreateItem(newItem);

        // generate response with location header
        var response = Created($"/api/items/{newItem.Id}", newItem);

        return response;
    }

    /**
     * PUT /api/items/{id}
     *
     * Updates item with given id.
     */
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Item item)
    {
        var oldItem = await _itemsRepository.Get(id);

        if (item.Id != id)
        {
            return BadRequest("Id in url and body must be the same.");
        }

        if (oldItem == null)
        {
            return NotFound($"Item with {id} not found.");
        }

        FillNewData(oldItem, item);

        await _itemsFacade.UpdateItem(oldItem);

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
        var dbItem = await _itemsRepository.Get(id);

        if (dbItem == null)
            return NotFound($"Item with {id} not found.");

        await _itemsRepository.Delete(dbItem);

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
        await _itemsFacade.AddCategory(item, dbCategory);

        return Created($"/api/items/{id}/categories/{dbCategory.Id}", null);
    }

    private void FillNewData(Item item, Item newData)
    {
        var editableProperties = typeof(Item)
            .GetProperties()
            .Where(property => !Attribute.IsDefined(property, typeof(ReadOnlyAttribute)))
            .ToList();

        foreach (var property in editableProperties)
        {
            var newValue = property.GetValue(newData);
            property.SetValue(item, newValue);
        }
    }
}
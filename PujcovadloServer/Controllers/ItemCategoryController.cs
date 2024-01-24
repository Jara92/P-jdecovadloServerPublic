using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PujcovadloServer.Exceptions;
using PujcovadloServer.Facades;
using PujcovadloServer.Filters;
using PujcovadloServer.Models;
using PujcovadloServer.Repositories.Interfaces;

namespace PujcovadloServer.Controllers;

[ApiController]
[Route("api/item-categories")]
public class ItemCategoryController : ControllerBase
{
    private readonly ItemCategoriesFacade _itemCategoryFacade;
    private readonly IItemCategoryRepository _itemCategoriesRepository;

    public ItemCategoryController(ItemCategoriesFacade itemCategoriesFacade, IItemCategoryRepository itemCategoriesRepository)
    {
        _itemCategoryFacade = itemCategoriesFacade;
        _itemCategoriesRepository = itemCategoriesRepository;
    }

    /**
     * GET /api/item-categories
     *
     * Returns all items.
     */
    [HttpGet]
    public async Task<ActionResult<List<ItemCategory>>> Index(ItemCategoryFilter filter)
    {
        var categories = await _itemCategoriesRepository.GetAll(filter);

        return Ok(categories);
    }

    /**
     * GET /api/item-categories/{id}
     *
     * Returns item with given id.
     */
    [HttpGet("{id}")]
    public async Task<ActionResult<ItemCategory>> Get(int id)
    {
        var categories = await _itemCategoriesRepository.Get(id);

        if (categories == null)
        {
            return NotFound($"ItemCategory with {id} not found.");
        }

        return Ok(categories);
    }

    /**
     * POST /api/item-categories
     *
     * Creates new item.
     */
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ItemCategory category)
    {
        await _itemCategoryFacade.Create(category);

        // generate response with location header
        var response = Created($"/api/item-categories/{category.Id}", category);

        return response;
    }

    /**
     * PUT /api/item-categories/{id}
     *
     * Updates item with given id.
     */
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] ItemCategory category)
    {
        // Check that id in url and body are the same
        if (category.Id != id)
        {
            return BadRequest("Id in url and body must be the same.");
        }

        try
        {
            await _itemCategoryFacade.Update(category);
        }
        catch (EntityNotFoundException)
        {
            return NotFound($"ItemCategory with {id} not found.");
        }
        catch (DbUpdateConcurrencyException)
        {
            return Conflict($"Concurrency exception. The item with id {id} has been updated in the meantime.");
        }

        return Ok();
    }

    /**
     * DELETE /api/item-categories/{id}
     *
     * Deletes item with given id.
     */
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var category = await _itemCategoriesRepository.Get(id);

        if (category == null)
        {
            return NotFound($"ItemCategory with {id} not found.");
        }

        await _itemCategoriesRepository.Delete(category);

        return NoContent();
    }

    [HttpGet("{id}/items")]
    public async Task<ActionResult<List<Item>>> GetItems(int id)
    {
        var category = await _itemCategoriesRepository.Get(id);

        if (category == null)
        {
            return NotFound($"ItemCategory with {id} not found.");
        }

        return Ok(category.Items.ToList());
    }
}
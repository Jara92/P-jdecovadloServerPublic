using System.ComponentModel;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using PujcovadloServer.Facades;
using PujcovadloServer.Models;
using PujcovadloServer.Repositories;
using PujcovadloServer.Services;

namespace PujcovadloServer.Controllers;

[ApiController]
[Route("api/item-categories")]
public class ItemCategoriesController : ControllerBase
{
    private readonly ItemCategoriesRepository _itemCategoriesRepository;

    public ItemCategoriesController(ItemCategoriesRepository itemCategoriesRepository)
    {
        _itemCategoriesRepository = itemCategoriesRepository;
    }

    /**
     * GET /api/item-categories
     *
     * Returns all items.
     */
    [HttpGet]
    public async Task<ActionResult<List<ItemCategory>>> Index()
    {
        var categories = await _itemCategoriesRepository.GetAll();

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
        var newCategory = new ItemCategory();
        FillNewData(newCategory, category);

        newCategory.Alias = UrlHelper.CreateUrlStub(newCategory.Name);
        await _itemCategoriesRepository.Create(newCategory);

        // generate response with location header
        var response = Created($"/api/item-categories/{newCategory.Id}", newCategory);

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
        var oldCategory = await _itemCategoriesRepository.Get(id);

        if (category.Id != id)
        {
            return BadRequest("Id in url and body must be the same.");
        }

        if (oldCategory == null)
        {
            return NotFound($"ItemCategory with {id} not found.");
        }

        FillNewData(oldCategory, category);

        oldCategory.Alias = UrlHelper.CreateUrlStub(oldCategory.Name);
        await _itemCategoriesRepository.Update(oldCategory);

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
            return NotFound($"ItemCategory with {id} not found.");

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

    private void FillNewData(ItemCategory category, ItemCategory newData)
    {
        var editableProperties = typeof(ItemCategory)
            .GetProperties()
            .Where(property => !Attribute.IsDefined(property, typeof(ReadOnlyAttribute)))
            .ToList();

        foreach (var property in editableProperties)
        {
            var newValue = property.GetValue(newData);
            property.SetValue(category, newValue);
        }
    }
}
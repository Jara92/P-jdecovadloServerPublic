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

    public ItemsController(ItemsRepository itemsRepository, ItemsFacade itemsFacade)
    {
        _itemsRepository = itemsRepository;
        _itemsFacade = itemsFacade;
    }

    /**
     * GET /api/items
     * 
     * Returns all items.
     */
    [HttpGet]
    public ActionResult<List<Item>> Index()
    {
        var items = _itemsRepository.GetAll().Result;

        return Ok(items);
    }

    /**
     * GET /api/items/{id}
     * 
     * Returns item with given id.
     */
    [HttpGet("{id}")]
    public ActionResult<Item> Get(int id)
    {
        var item = _itemsRepository.Get(id).Result;

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
    public IActionResult Create([FromBody] Item item)
    {
        var newItem = new Item();
        FillNewData(newItem, item);
        
        _itemsFacade.CreateItem(newItem);

        // generate response with location header
        var response = Created();
        response.Location = $"/api/items/{newItem.Id}";
        
        return response;
    }

    /**
     * PUT /api/items/{id}
     * 
     * Updates item with given id.
     */
    [HttpPut("{id}")]
    public IActionResult Update(int id, [FromBody] Item item)
    {
        var oldItem = _itemsRepository.Get(id).Result;

        if (item.Id != id)
        {
            return BadRequest("Id in url and body must be the same.");
        }
        
        if (oldItem == null)
        {
            return NotFound($"Item with {id} not found.");
        }
        
        FillNewData(oldItem, item);

        _itemsFacade.UpdateItem(oldItem);
        
        return Ok();
    }
    
    /**
     * DELETE /api/items/{id}
     * 
     * Deletes item with given id.
     */
    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        var dbItem = _itemsRepository.Get(id).Result;
        
        if(dbItem == null)
            return NotFound($"Item with {id} not found.");
        
        _itemsRepository.Delete(dbItem);

        return NoContent();
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
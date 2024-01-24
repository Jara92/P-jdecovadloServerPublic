using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PujcovadloServer.Business.Entities;
using PujcovadloServer.Business.Exceptions;
using PujcovadloServer.Business.Facades;
using PujcovadloServer.Business.Filters;
using PujcovadloServer.Business.Interfaces;
using PujcovadloServer.Responses;

namespace PujcovadloServer.Api.Controllers;

[ApiController]
[Route("api/item-categories")]
public class ItemCategoryController : ACrudController
{
    private readonly ItemCategoryFacade _itemCategoryFacade;
    private readonly IItemCategoryRepository _itemCategoriesRepository;
    private readonly IMapper _mapper;
    private readonly LinkGenerator _urlHelper;

    public ItemCategoryController(ItemCategoryFacade itemCategoryFacade, IItemCategoryRepository itemCategoriesRepository,
        IMapper mapper, LinkGenerator linkGenerator) : base(linkGenerator)
    {
        _itemCategoryFacade = itemCategoryFacade;
        _itemCategoriesRepository = itemCategoriesRepository;
        _mapper = mapper;
        _urlHelper = linkGenerator;
    }

    /**
     * GET /api/item-categories
     *
     * Returns all items.
     */
    [HttpGet]
    public async Task<ActionResult<List<ItemCategoryResponse>>> Index([FromQuery]ItemCategoryFilter filter)
    {
        // get categories by filter
        var categories = await _itemCategoriesRepository.GetAll(filter);
        
        // Convert to response items
        var categoriesResponse = _mapper.Map<List<ItemCategoryResponse>>(categories);
        
        // Hateos item links
        foreach (var category in categoriesResponse)
        {
            category.Links.Add(new LinkResponse(
                _urlHelper.GetUriByAction(HttpContext, nameof(this.Get), values: new { category.Id }), "SELF", "GET"));
            
            category.Links.Add(new LinkResponse(
                _urlHelper.GetUriByAction(HttpContext, nameof(ItemController.Index), "Item", values: new { CategoryId = category.Id }), "ITEMS", "GET"));
        }
        
        var links = GeneratePaginationLinks(categories, filter, nameof(Index));

        // Create response list
        var response = new ResponseList<ItemCategoryResponse>
        {
            Data = categoriesResponse,
            Links = links
        };

        return Ok(response);
    }

    /**
     * GET /api/item-categories/{id}
     *
     * Returns item with given id.
     */
    [HttpGet("{id}")]
    public async Task<ActionResult<ItemCategoryResponse>> Get(int id)
    {
        // Get the category
        var category = await _itemCategoriesRepository.Get(id);
        if (category == null) return NotFound($"ItemCategory with {id} not found.");
        
        // Map to response
        var categoryResponse = _mapper.Map<ItemCategoryResponse>(category);
        
        // Link to items
        categoryResponse.Links.Add(new LinkResponse(
            _urlHelper.GetUriByAction(HttpContext, nameof(ItemController.Index), "Item", values: new { CategoryId = category.Id }), "ITEMS", "GET"));
        
        return Ok(categoryResponse);
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
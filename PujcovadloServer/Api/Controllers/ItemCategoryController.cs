using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using PujcovadloServer.Business.Facades;
using PujcovadloServer.Business.Filters;
using PujcovadloServer.Business.Services;
using PujcovadloServer.Responses;

namespace PujcovadloServer.Api.Controllers;

[ApiController]
[Route("api/item-categories")]
public class ItemCategoryController : ACrudController
{
    private readonly ItemCategoryFacade _itemCategoryFacade;
    private readonly ItemCategoryService _itemCategoryService;
    private readonly IMapper _mapper;
    private readonly LinkGenerator _urlHelper;

    public ItemCategoryController(ItemCategoryFacade itemCategoryFacade, ItemCategoryService itemCategoryService,
        IMapper mapper, LinkGenerator linkGenerator) : base(linkGenerator)
    {
        _itemCategoryFacade = itemCategoryFacade;
        _itemCategoryService = itemCategoryService;
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
        var categories = await _itemCategoryService.GetAll(filter);
        
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
        var category = await _itemCategoryService.Get(id);
        if (category == null) return NotFound($"ItemCategory with {id} not found.");
        
        // Map to response
        var categoryResponse = _mapper.Map<ItemCategoryResponse>(category);
        
        // Link to items
        categoryResponse.Links.Add(new LinkResponse(
            _urlHelper.GetUriByAction(HttpContext, nameof(ItemController.Index), "Item", values: new { CategoryId = category.Id }), "ITEMS", "GET"));
        
        return Ok(categoryResponse);
    }
}
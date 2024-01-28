using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PujcovadloServer.Api.Filters;
using PujcovadloServer.Business.Facades;
using PujcovadloServer.Business.Filters;
using PujcovadloServer.Business.Services;
using PujcovadloServer.Responses;

namespace PujcovadloServer.Api.Controllers;

[ApiController]
[Route("api/item-categories")]
[ServiceFilter(typeof(ExceptionFilter))]
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

    /// <summary>
    /// Returns paginated list of categories by given filter.
    /// </summary>
    /// <param name="filter">Filter</param>
    /// <returns></returns>
    /// <response code="200">Returns paginated list of categories.</response>
    /// <response code="400">If filter input is invalid.</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [Authorize]
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

    /// <summary>
    /// Returns category by given id.
    /// </summary>
    /// <param name="id">Categorie's id.</param>
    /// <returns></returns>
    /// <response code="200">Returns category.</response>
    /// <response code="404">If category with given id is not found.</response>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Authorize]
    public async Task<ActionResult<ItemCategoryResponse>> Get(int id)
    {
        // Get the category
        var category = await _itemCategoryFacade.Get(id);
        
        // Map to response
        var categoryResponse = _mapper.Map<ItemCategoryResponse>(category);
        
        // Link to items
        categoryResponse.Links.Add(new LinkResponse(
            _urlHelper.GetUriByAction(HttpContext, nameof(ItemController.Index), "Item", values: new { CategoryId = category.Id }), "ITEMS", "GET"));
        
        return Ok(categoryResponse);
    }
}
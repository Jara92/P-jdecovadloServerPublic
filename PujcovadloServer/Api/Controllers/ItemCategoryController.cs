using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PujcovadloServer.Api.Filters;
using PujcovadloServer.Api.Services;
using PujcovadloServer.AuthorizationHandlers;
using PujcovadloServer.AuthorizationHandlers.ItemCategory;
using PujcovadloServer.Business.Entities;
using PujcovadloServer.Business.Facades;
using PujcovadloServer.Business.Filters;
using PujcovadloServer.Business.Services;
using PujcovadloServer.Responses;

namespace PujcovadloServer.Api.Controllers;

[ApiController]
[Route("api/item-categories")]
[ServiceFilter(typeof(ExceptionFilter))]
public class ItemCategoryController : ACrudController<ItemCategory>
{
    private readonly ItemCategoryFacade _itemCategoryFacade;
    private readonly ItemCategoryService _itemCategoryService;
    private readonly ItemCategoryResponseGenerator _categoryResponseGenerator;
    private readonly IMapper _mapper;
    private readonly LinkGenerator _urlHelper;

    public ItemCategoryController(ItemCategoryFacade itemCategoryFacade, ItemCategoryService itemCategoryService,
        ItemCategoryResponseGenerator categoryResponseGenerator, IMapper mapper, LinkGenerator linkGenerator,
        AuthorizationService authorizationService) : base(authorizationService, linkGenerator)
    {
        _itemCategoryFacade = itemCategoryFacade;
        _itemCategoryService = itemCategoryService;
        _categoryResponseGenerator = categoryResponseGenerator;
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
    public async Task<ActionResult<List<ItemCategoryResponse>>> Index([FromQuery] ItemCategoryFilter filter)
    {
        // get categories by filter
        var categories = await _itemCategoryService.GetAll(filter);

        // Generate response
        var response =
            await _categoryResponseGenerator.GenerateResponseList(categories, filter, nameof(Index), "ItemCategory");

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

        await _authorizationService.CheckPermissions(category, ItemCategoryOperations.Read);

        // Generate response
        var categoryResponse = await _categoryResponseGenerator.GenerateItemCategoryDetailResponse(category);

        return Ok(categoryResponse);
    }
}
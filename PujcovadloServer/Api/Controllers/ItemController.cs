using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PujcovadloServer.Api.Filters;
using PujcovadloServer.Api.Services;
using PujcovadloServer.AuthorizationHandlers;
using PujcovadloServer.Business.Entities;
using PujcovadloServer.Business.Enums;
using PujcovadloServer.Business.Facades;
using PujcovadloServer.Business.Filters;
using PujcovadloServer.Business.Interfaces;
using PujcovadloServer.Business.Services;
using PujcovadloServer.Requests;
using PujcovadloServer.Responses;

namespace PujcovadloServer.Api.Controllers;

[ApiController]
[Route("api/items")]
[ServiceFilter(typeof(ExceptionFilter))]
public class ItemController : ACrudController<Item>
{
    private readonly IMapper _mapper;
    private readonly IItemRepository _itemsRepository;
    private readonly ItemService _itemService;
    private readonly ItemFacade _itemFacade;
    private readonly ItemCategoryFacade _itemCategoryFacade;
    private readonly ItemResponseGenerator _itemResponseGenerator;

    public ItemController(IItemRepository itemsRepository, ItemFacade itemFacade,
        ItemCategoryFacade itemCategoryFacade, IMapper mapper,
        ItemService itemService, LinkGenerator urlHelper, ItemResponseGenerator itemResponseGenerator,
        AuthorizationService authorizationService) : base(authorizationService, urlHelper)
    {
        _itemsRepository = itemsRepository;
        _itemFacade = itemFacade;
        _itemCategoryFacade = itemCategoryFacade;
        _mapper = mapper;
        _itemService = itemService;
        _itemResponseGenerator = itemResponseGenerator;
    }

    /// <summary>
    /// Returns all items by given filter.
    /// </summary>
    /// <param name="filter">Filtering object</param>
    /// <returns>Paginated, filtered and sorted items.</returns>
    /// <response code="200">Returns paginated list of items.</response>
    /// <response code="400">If filter input is invalid.</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<List<ItemResponse>>> Index([FromQuery] ItemFilter filter)
    {
        // Get items
        var items = await _itemService.GetAll(filter);

        // get response list
        var response = await _itemResponseGenerator.GenerateResponseList(items, filter, nameof(Get), "Item");


        return Ok(response);
    }

    /// <summary>
    /// Returns item with given id.
    /// </summary>
    /// <param name="id">Item's id.</param>
    /// <returns>Item identified by the id.</returns>
    /// <response code="200">Returns item with given id.</response>
    /// <response code="404">If item with given id was not found.</response>
    [HttpGet("{id}", Name = nameof(Get))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ItemDetailResponse>> Get(int id)
    {
        // Get the item and convert it to response
        var item = await _itemFacade.GetItem(id);
        
        await _authorizationService.CheckPermissions(item, ItemAuthorizationHandler.Operations.Read);

        // get item response
        var responseItem = await _itemResponseGenerator.GenerateItemDetailResponse(item);

        return Ok(responseItem);
    }
    
        /// <summary>
    /// Create a new item.
    /// </summary>
    /// <param name="request">New Item</param>
    /// <returns>Newly created item.</returns>
    /// <response code="201">Returns the newly created item.</response>
    /// <response code="400">If the item is invalid.</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [Authorize(Roles = UserRoles.Owner)]
    public async Task<ActionResult<ItemResponse>> Create([FromBody] ItemRequest request)
    {
        await _authorizationService.CheckPermissions(_mapper.Map<Item>(request),
            ItemAuthorizationHandler.Operations.Create);

        var newItem = await _itemFacade.CreateItem(request);

        // generate detailed reponse for the owner
        var responseItem = await _itemResponseGenerator.GenerateItemOwnerResponse(newItem);

        // generate response with location header
        return CreatedAtAction(_urlHelper.GetUriByAction(HttpContext, nameof(Get), values: newItem.Id), responseItem);
    }

    /// <summary>
    /// Updates item with given id.
    /// </summary>
    /// <param name="id">Item's id.</param>
    /// <param name="request">Updated item.</param>
    /// <returns></returns>
    /// <response code="204">If the item was updated successfully.</response>
    /// <response code="400">If the item data is invalid.</response>
    /// <response code="404">If the item was not found.</response>
    /// <response code="409">If the item was updated in the meantime.</response>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ValidateIdFilter]
    [Authorize(Roles = UserRoles.Owner)]
    public async Task<IActionResult> Update(int id, [FromBody] ItemRequest request)
    {
        var item = await _itemFacade.GetItem(id);
        
        await _authorizationService.CheckPermissions(item, ItemAuthorizationHandler.Operations.Update);

        // Update the item
        await _itemFacade.UpdateItem(item, request);

        return NoContent();
    }

    /// <summary>
    /// Deletes item with given id.
    /// </summary>
    /// <param name="id">item's id</param>
    /// <returns></returns>
    /// <response code="204">If the item was deleted successfully.</response>
    /// <response code="404">If the item was not found.</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        // Get the item
        var item = await _itemFacade.GetItem(id);

        await _authorizationService.CheckPermissions(item, ItemAuthorizationHandler.Operations.Delete);

        await _itemFacade.DeleteItem(item);

        return NoContent();
    }

    /// <summary>
    /// Returns item's categories.
    /// </summary>
    /// <param name="id">Item's id</param>
    /// <returns>All categories related to the item.</returns>
    /// <response code="200">Returns all categories related to the item.</response>
    /// <response code="404">If the item was not found.</response>
    [HttpGet("{id}/categories")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<ItemCategoryResponse>>> GetCategories(int id)
    {
        // Get item
        var item = await _itemsRepository.Get(id);
        if (item == null) return NotFound($"Item with {id} not found.");

        // Get categories
        var categoriesResponse = _mapper.Map<List<ItemCategoryResponse>>(item.Categories);

        return Ok(categoriesResponse);
    }
}
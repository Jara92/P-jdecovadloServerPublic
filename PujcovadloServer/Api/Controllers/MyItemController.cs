using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PujcovadloServer.Api.Filters;
using PujcovadloServer.AuthorizationHandlers;
using PujcovadloServer.Business.Entities;
using PujcovadloServer.Business.Enums;
using PujcovadloServer.Business.Facades;
using PujcovadloServer.Business.Filters;
using PujcovadloServer.Business.Services;
using PujcovadloServer.Responses;

namespace PujcovadloServer.Api.Controllers;

[ApiController]
[Route("api/my-items")]
[Authorize(Roles = UserRoles.Owner)]
[ServiceFilter(typeof(ExceptionFilter))]
public class MyItemController : ACrudController<Item>
{
    private readonly ItemService _itemService;
    private readonly ItemFacade _itemFacade;
    private readonly ImageFacade _imageFacade;
    private readonly IMapper _mapper;

    public MyItemController(ItemFacade itemFacade, ImageFacade imageFacade, ItemService itemService,
        LinkGenerator urlHelper, IMapper mapper, AuthorizationService authorizationService) : base(authorizationService, urlHelper)
    {
        _itemService = itemService;
        _itemFacade = itemFacade;
        _imageFacade = imageFacade;
        _mapper = mapper;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ItemResponse>> Index([FromQuery] ItemFilter filter)
    {
        // Get items
        var items = await _itemFacade.GetMyItems(filter);

        // Map items to response
        var responseItems = _mapper.Map<List<ItemResponse>>(items);

        // Hateos item links
        foreach (var item in responseItems)
        {
            item.Links.Add(new LinkResponse(
                _urlHelper.GetUriByAction(HttpContext, nameof(MyItemController.Get), "MyItem", 
                    values: new { item.Id }), "SELF", "GET"));
            item.Links.Add(new LinkResponse(
                _urlHelper.GetUriByAction(HttpContext, nameof(ItemController.Update), "Item",
                    values: new { item.Id }),
                "UPDATE", "PUT"));
            item.Links.Add(new LinkResponse(
                _urlHelper.GetUriByAction(HttpContext, nameof(ItemController.Delete), "Item",
                    values: new { item.Id }),
                "DELETE", "DELETE"));
        }

        // Generate pagination links
        var links = GeneratePaginationLinks(items, filter, nameof(Index));

        // Return response
        return Ok(new ResponseList<ItemResponse>
        {
            Data = responseItems,
            Links = links
        });
    }

    /// <summary>
    /// Returns full item by given id.
    /// </summary>
    /// <param name="id">Id of the item</param>
    /// <returns>Returns all information about the item.</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ItemOwnerResponse>> Get(int id)
    {
        var item = await _itemFacade.GetItem(id);

        // Can read all item's data only if the user is can update the item
        await _authorizationService.CheckPermissions(item, ItemAuthorizationHandler.Operations.Update);

        var response = _mapper.Map<ItemOwnerResponse>(item);

        response.Links.Add(new LinkResponse(
            _urlHelper.GetUriByAction(HttpContext, nameof(MyItemController.Get), "MyItem", values: new { response.Id }),
            "SELF", "GET"));

        return Ok(response);
    }
}
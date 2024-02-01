using System.Security.Claims;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PujcovadloServer.Api.Filters;
using PujcovadloServer.Authentication;
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
    private readonly IMapper _mapper;

    public MyItemController(ItemFacade itemFacade, ItemService itemService, LinkGenerator urlHelper, IMapper mapper,
        IAuthorizationService authorizationService) : base(authorizationService, urlHelper)
    {
        _itemService = itemService;
        _itemFacade = itemFacade;
        _mapper = mapper;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ItemOwnerResponse>> Index([FromQuery] ItemFilter filter)
    {
        // Get items
        var items = await _itemFacade.GetMyItems(filter);

        // Map items to response
        var responseItems = _mapper.Map<List<ItemOwnerResponse>>(items);

        // Hateos item links
        foreach (var item in responseItems)
        {
            item.Links.Add(new LinkResponse(
                _urlHelper.GetUriByAction(HttpContext, nameof(ItemController.Get), "Item", values: new { item.Id }),
                "SELF", "GET"));
            item.Links.Add(new LinkResponse(
                _urlHelper.GetUriByAction(HttpContext, nameof(ItemController.Update), "Item", values: new { item.Id }),
                "UPDATE", "PUT"));
            item.Links.Add(new LinkResponse(
                _urlHelper.GetUriByAction(HttpContext, nameof(ItemController.Delete), "Item", values: new { item.Id }),
                "DELETE", "DELETE"));
        }

        // Generate pagination links
        var links = GeneratePaginationLinks(items, filter, nameof(Index));

        // Return response
        return Ok(new ResponseList<ItemOwnerResponse>
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
        var item = await _itemFacade.GetMyItem(id);

        await CheckPermissions(item, ItemAuthorizationHandler.Operations.Read);

        var response = _mapper.Map<ItemOwnerResponse>(item);

        response.Links.Add(new LinkResponse(
            _urlHelper.GetUriByAction(HttpContext, nameof(MyItemController.Get), "MyItem", values: new { response.Id }),
            "SELF", "GET"));

        return Ok(response);
    }
}
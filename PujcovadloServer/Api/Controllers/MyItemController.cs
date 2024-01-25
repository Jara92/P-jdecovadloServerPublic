using System.Security.Claims;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PujcovadloServer.Authentication;
using PujcovadloServer.Business.Enums;
using PujcovadloServer.Business.Facades;
using PujcovadloServer.Business.Filters;
using PujcovadloServer.Business.Services;
using PujcovadloServer.Responses;

namespace PujcovadloServer.Api.Controllers;

[ApiController]
[Route("api/my-items")]
public class MyItemController : ACrudController
{
    private readonly ItemService _itemService;
    private readonly ItemFacade _itemFacade;
    private readonly IMapper _mapper;
    private readonly AuthenticateService _authenticateService;

    public MyItemController(ItemFacade itemFacade, ItemService itemService, LinkGenerator urlHelper, IMapper mapper,
        AuthenticateService authenticateService) : base(urlHelper)
    {
        _itemService = itemService;
        _itemFacade = itemFacade;
        _mapper = mapper;
        _authenticateService = authenticateService;
    }

    [HttpGet]
    [Authorize(Roles = UserRoles.Owner)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> Index([FromQuery] ItemFilter filter)
    {
        // Get current user
        // TODO: Catch exception when user is not found - should not happen
        var user = await _authenticateService.GetUser(User);

        // Get items
        var items = await _itemFacade.GetMyItems(filter, user);

        // Map items to response
        var responseItems = _mapper.Map<List<ItemResponse>>(items);

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
        return Ok(new ResponseList<ItemResponse>
        {
            Data = responseItems,
            Links = links
        });
    }
}
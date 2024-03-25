using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PujcovadloServer.Areas.Admin.Business.Facades;
using PujcovadloServer.Areas.Api.Filters;
using PujcovadloServer.Areas.Api.Services;
using PujcovadloServer.AuthorizationHandlers;
using PujcovadloServer.Business.Entities;
using PujcovadloServer.Business.Filters;
using PujcovadloServer.Responses;

namespace PujcovadloServer.Areas.Api.Controllers;

[Area("Api")]
[ApiController]
[Route("api/item-tags")]
[ServiceFilter(typeof(ExceptionFilter))]
public class ItemTagController : ACrudController<ItemTag>
{
    private readonly ItemTagFacade _itemTagFacade;
    private readonly ItemTagResponseGenerator _tagResponseGenerator;

    public ItemTagController(ItemTagFacade itemTagFacade, ItemTagResponseGenerator tagResponseGenerator,
        LinkGenerator linkGenerator, AuthorizationService authorizationService) : base(authorizationService,
        linkGenerator)
    {
        _itemTagFacade = itemTagFacade;
        _tagResponseGenerator = tagResponseGenerator;
    }

    /// <summary>
    /// Returns paginated list of tags by given filter.
    /// </summary>
    /// <param name="filter">Filter</param>
    /// <returns></returns>
    /// <response code="200">Returns paginated list of tags.</response>
    /// <response code="400">If filter input is invalid.</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [Authorize]
    public async Task<ActionResult<List<ItemTagResponse>>> Index([FromQuery] ItemTagFilter filter)
    {
        // get tags by filter
        var tags = await _itemTagFacade.GetAll(filter);

        // Generate response
        var response =
            await _tagResponseGenerator.GenerateResponseList(tags, filter, nameof(Index), "ItemTag");

        return Ok(response);
    }
}
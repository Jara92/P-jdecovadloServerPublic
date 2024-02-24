using Microsoft.AspNetCore.Mvc;
using PujcovadloServer.AuthorizationHandlers;
using PujcovadloServer.Business.Entities;
using PujcovadloServer.Business.Filters;
using PujcovadloServer.Lib;
using PujcovadloServer.Responses;

namespace PujcovadloServer.Areas.Api.Controllers;

public abstract class ACrudController<T> : ControllerBase where T : BaseEntity
{
    protected readonly AuthorizationService _authorizationService;
    protected readonly LinkGenerator _urlHelper;

    public ACrudController(AuthorizationService authorizationService, LinkGenerator urlHelper)
    {
        _authorizationService = authorizationService;
        _urlHelper = urlHelper;
    }

    protected IList<LinkResponse> GeneratePaginationLinks(IPaginatedList items, BaseFilter filter,
        string action)
    {
        var links = new List<LinkResponse>();

        // Next page link
        if (items.HasNextPage)
        {
            var nextPageFilter = filter.Clone();
            nextPageFilter.Page = items.PageIndex + 1;

            links.Add(new LinkResponse(
                _urlHelper.GetUriByAction(HttpContext, action, values: nextPageFilter), "NEXT", "GET"));
        }

        // Previous page link
        if (items.HasPreviousPage)
        {
            var previousPageFilter = filter.Clone();
            previousPageFilter.Page = items.PageIndex - 1;

            links.Add(new LinkResponse(
                _urlHelper.GetUriByAction(HttpContext, action, values: previousPageFilter), "PREVIOUS", "GET"));
        }

        return links;
    }
}
using Microsoft.AspNetCore.Mvc;
using PujcovadloServer.Business.Entities;
using PujcovadloServer.Business.Filters;
using PujcovadloServer.Lib;
using PujcovadloServer.Responses;

namespace PujcovadloServer.Api.Controllers;

public abstract class ACrudController : ControllerBase
{
    protected readonly LinkGenerator _urlHelper;

    public ACrudController(LinkGenerator urlHelper)
    {
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
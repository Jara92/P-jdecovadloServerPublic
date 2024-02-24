using PujcovadloServer.AuthorizationHandlers;
using PujcovadloServer.Business.Filters;
using PujcovadloServer.Lib;
using PujcovadloServer.Responses;

namespace PujcovadloServer.Areas.Api.Services;

public abstract class ABaseResponseGenerator
{
    protected readonly LinkGenerator _urlHelper;
    protected readonly IHttpContextAccessor _httpContextAccessor;
    protected readonly AuthorizationService _authorizationService;
    protected HttpContext _httpContext;

    public ABaseResponseGenerator(IHttpContextAccessor httpContextAccessor, LinkGenerator urlHelper,
        AuthorizationService authorizationService)
    {
        _urlHelper = urlHelper;
        _httpContextAccessor = httpContextAccessor;
        _authorizationService = authorizationService;

        _httpContext = _httpContextAccessor.HttpContext;
    }

    /// <summary>
    /// Generates pagination links for input list using input filter, action and controller.
    /// </summary>
    /// <param name="list">List of data</param>
    /// <param name="filter">Filter query for pagination.</param>
    /// <param name="action">Pagination action.</param>
    /// <param name="controller">Pagination controller</param>
    /// <returns></returns>
    protected List<LinkResponse> GeneratePaginationLinks(IPaginatedList list, BaseFilter filter, string action,
        string controller)
    {
        var links = new List<LinkResponse>();

        // Next page link
        if (list.HasNextPage)
        {
            var nextPageFilter = filter.Clone();
            nextPageFilter.Page = list.PageIndex + 1;

            links.Add(new LinkResponse(
                _urlHelper.GetUriByAction(_httpContext, action, controller, values: nextPageFilter), "NEXT", "GET"));
        }

        // Previous page link
        if (list.HasPreviousPage)
        {
            var previousPageFilter = filter.Clone();
            previousPageFilter.Page = list.PageIndex - 1;

            links.Add(new LinkResponse(
                _urlHelper.GetUriByAction(_httpContext, action, controller, values: previousPageFilter), "PREVIOUS",
                "GET"));
        }

        return links;
    }
}
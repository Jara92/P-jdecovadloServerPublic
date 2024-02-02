using PujcovadloServer.Business.Entities;
using PujcovadloServer.Business.Filters;
using PujcovadloServer.Lib;
using PujcovadloServer.Responses;

namespace PujcovadloServer.Api.Services;

public abstract class AHateoasGenerator<TEntity, TResponse>
    where TEntity : BaseEntity where TResponse : class
{
    
    protected readonly IHttpContextAccessor _httpContextAccessor;
    protected readonly LinkGenerator _urlHelper;
    
    public AHateoasGenerator(IHttpContextAccessor httpContextAccessor, LinkGenerator urlHelper)
    {
        _httpContextAccessor = httpContextAccessor;
        _urlHelper = urlHelper;
    }
    
    public abstract void AddLinks(TResponse response, TEntity entity);

    public abstract ResponseList<TResponse> GetWithLinks(IList<TResponse> responses, PaginatedList<TEntity> entities);

    public virtual ResponseList<TResponse> GetWithPagination(IList<TResponse> responses, PaginatedList<TEntity> entities, BaseFilter filter, string action)
    {
        var response = GetWithLinks(responses, entities);

        // Next page link
        if (entities.HasNextPage)
        {
            var nextPageFilter = filter.Clone();
            nextPageFilter.Page = entities.PageIndex + 1;

            response.Links.Add(new LinkResponse(
                _urlHelper.GetUriByAction(_httpContextAccessor.HttpContext, action, 
                    values: nextPageFilter), "NEXT", "GET"));
        }

        // Previous page link
        if (entities.HasPreviousPage)
        {
            var previousPageFilter = filter.Clone();
            previousPageFilter.Page = entities.PageIndex - 1;

            response.Links.Add(new LinkResponse(
                _urlHelper.GetUriByAction(_httpContextAccessor.HttpContext, action, 
                    values: previousPageFilter), "PREVIOUS", "GET"));
        }

        return response;
    }
}
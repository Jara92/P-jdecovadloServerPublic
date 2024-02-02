using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using PujcovadloServer.Authentication.Exceptions;
using PujcovadloServer.AuthorizationHandlers.Exceptions;
using PujcovadloServer.Business.Entities;
using PujcovadloServer.Business.Filters;
using PujcovadloServer.Lib;
using PujcovadloServer.Responses;

namespace PujcovadloServer.Api.Controllers;

public abstract class ACrudController<T> : ControllerBase where T : BaseEntity
{
    protected readonly IAuthorizationService _authorizationService;
    protected readonly LinkGenerator _urlHelper;

    public ACrudController(IAuthorizationService authorizationService, LinkGenerator urlHelper)
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
    
    /// <summary>
    /// Checks if the user has permissions to perform the operation on the entity.
    /// </summary>
    /// <param name="entity">The entity.</param>
    /// <param name="requirement">Required action</param>
    /// <exception cref="ForbiddenAccessException">User does not have permission to perform the action.</exception>
    /// <exception cref="UnauthorizedAccessException">User is not authorized.</exception>
    protected async Task CheckPermissions(T entity, OperationAuthorizationRequirement requirement)
    {
        await CheckPermissions<T>(entity, requirement);
    }

    /// <summary>
    /// Checks if the user has permissions to perform the operation on the entity of given type.
    /// </summary>
    /// <param name="entity">The entity.</param>
    /// <param name="requirement">Required action</param>
    /// <typeparam name="TE">Entity type to be checked.</typeparam>
    /// <exception cref="ForbiddenAccessException">User does not have permission to perform the action.</exception>
    /// <exception cref="UnauthorizedAccessException">User is not authorized.</exception>
    protected async Task CheckPermissions<TE>(TE entity, OperationAuthorizationRequirement requirement)
        where TE : BaseEntity
    {
        // Check requirement permissions
        var authorizationResult = await _authorizationService.AuthorizeAsync(User, entity, requirement);

        // Throw exception if not authorized
        if (!authorizationResult.Succeeded)
        {
            var identity = User.Identity;

            // Throw UnauthorizedAccessException if not authenticated
            if (identity == null || !identity.IsAuthenticated)
                throw new NotAuthenticatedException();

            // Throw ForbiddenAccessException if not authorized
            throw new ForbiddenAccessException("You are not authorized to perform this operation.");
        }
    }
}
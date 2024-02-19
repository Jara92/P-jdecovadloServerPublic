using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using PujcovadloServer.Authentication.Exceptions;
using PujcovadloServer.AuthorizationHandlers.Exceptions;
using PujcovadloServer.Business.Entities;

// TODO: Move some other namespace
namespace PujcovadloServer.AuthorizationHandlers;

public class AuthorizationService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAuthorizationService _authorizationService;

    public AuthorizationService(IHttpContextAccessor httpContextAccessor, IAuthorizationService authorizationService)
    {
        _httpContextAccessor = httpContextAccessor;
        _authorizationService = authorizationService;
    }

    /// <summary>
    /// Checks if the user has permissions to perform the operation on the entity of given type.
    /// </summary>
    /// <param name="entity">The entity.</param>
    /// <param name="requirement">Required action</param>
    /// <typeparam name="TE">Entity type to be checked.</typeparam>
    /// <exception cref="ForbiddenAccessException">User does not have permission to perform the action.</exception>
    /// <exception cref="NotAuthenticatedException">User is not authenticated.</exception>
    public async Task CheckPermissions<TE>(TE entity, OperationAuthorizationRequirement requirement)
        where TE : BaseEntity
    {
        // get User ClaimsPrincipal and check if it is not null
        var user = _httpContextAccessor.HttpContext?.User;
        if (user == null) throw new NotAuthenticatedException();

        // Check requirement permissions
        var authorizationResult = await _authorizationService.AuthorizeAsync(user, entity, requirement);

        // Throw exception if not authorized
        if (!authorizationResult.Succeeded)
        {
            var identity = user.Identity;

            // Throw NotAuthenticatedException if not authenticated
            if (identity == null || !identity.IsAuthenticated)
                throw new NotAuthenticatedException();

            // Get failure reasons
            var failureReasons =
                authorizationResult.Failure.FailureReasons.Select(failureReason => failureReason.ToString());

            // Throw ForbiddenAccessException if not authorized
            throw new ForbiddenAccessException(
                "You are not authorized to perform this operation.",
                failureReasons.ToArray()
            );
        }
    }

    public async Task<bool> CanPerformOperation<TE>(TE entity, OperationAuthorizationRequirement requirement)
        where TE : BaseEntity
    {
        // get User ClaimsPrincipal and check if it is not null
        var user = _httpContextAccessor.HttpContext?.User;
        if (user == null) throw new NotAuthenticatedException();

        // Check requirement permissions
        var authorizationResult = await _authorizationService.AuthorizeAsync(user, entity, requirement);

        return authorizationResult.Succeeded;
    }
}
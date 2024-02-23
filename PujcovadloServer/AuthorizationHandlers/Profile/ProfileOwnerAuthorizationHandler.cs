using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using PujcovadloServer.Business.Services.Interfaces;

namespace PujcovadloServer.AuthorizationHandlers.Profile;

public class
    ProfileOwnerAuthorizationHandler : AuthorizationHandler<OperationAuthorizationRequirement,
    Business.Entities.Profile>
{
    private readonly IAuthenticateService _authenticateService;

    public ProfileOwnerAuthorizationHandler(IAuthenticateService authenticateService)
    {
        _authenticateService = authenticateService;
    }

    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
        OperationAuthorizationRequirement requirement,
        Business.Entities.Profile resource)
    {
        var userId = _authenticateService.TryGetCurrentUserId();

        // Not authenticated or not the owner
        if (userId == null || resource.User.Id != userId)
        {
            return Task.CompletedTask;
        }

        // Owner can read his own profile
        if (requirement.Name == ProfileOperations.Constants.ReadOperationName)
        {
            context.Succeed(requirement);
        }

        // Owner can update his own profile
        if (requirement.Name == ProfileOperations.Constants.UpdateOperationName)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
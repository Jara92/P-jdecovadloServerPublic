using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using PujcovadloServer.Business.Enums;
using PujcovadloServer.Business.Services.Interfaces;

namespace PujcovadloServer.AuthorizationHandlers.Item;

public class
    ItemHasRoleOwnerAuthorizationHandler : AuthorizationHandler<OperationAuthorizationRequirement,
    Business.Entities.Item>
{
    private readonly IAuthenticateService _authenticateService;

    public ItemHasRoleOwnerAuthorizationHandler(IAuthenticateService authenticateService)
    {
        _authenticateService = authenticateService;
    }

    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
        OperationAuthorizationRequirement requirement,
        Business.Entities.Item resource)
    {
        // user not authenticated or not owner
        if (!context.User.IsInRole(UserRoles.Owner))
        {
            return Task.CompletedTask;
        }

        // user is owner so he can create an item
        if (requirement.Name == ItemOperations.Constants.CreateOperationName)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
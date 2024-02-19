using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using PujcovadloServer.Business.Enums;
using PujcovadloServer.Business.Services.Interfaces;

namespace PujcovadloServer.AuthorizationHandlers.Item;

public class
    ItemGuestAuthorizationHandler : AuthorizationHandler<OperationAuthorizationRequirement, Business.Entities.Item>
{
    private readonly IAuthenticateService _authenticateService;

    public ItemGuestAuthorizationHandler(IAuthenticateService authenticateService)
    {
        _authenticateService = authenticateService;
    }

    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
        OperationAuthorizationRequirement requirement,
        Business.Entities.Item resource)
    {
        // The item must be public
        if (resource.Status != ItemStatus.Public)
        {
            return Task.CompletedTask;
        }

        // Allow guests to read the item
        if (requirement.Name == ItemOperations.Constants.ReadOperationName)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
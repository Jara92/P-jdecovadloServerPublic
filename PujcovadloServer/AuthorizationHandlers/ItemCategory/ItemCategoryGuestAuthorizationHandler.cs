using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using PujcovadloServer.AuthorizationHandlers.Item;
using PujcovadloServer.Business.Services.Interfaces;

namespace PujcovadloServer.AuthorizationHandlers.ItemCategory;

public class
    ItemCategoryGuestAuthorizationHandler : AuthorizationHandler<OperationAuthorizationRequirement,
    Business.Entities.ItemCategory>
{
    private readonly IAuthenticateService _authenticateService;

    public ItemCategoryGuestAuthorizationHandler(IAuthenticateService authenticateService)
    {
        _authenticateService = authenticateService;
    }

    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
        OperationAuthorizationRequirement requirement,
        Business.Entities.ItemCategory resource)
    {
        // Anyone can read item categories
        if (requirement.Name == nameof(ItemCategoryOperations.Read))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
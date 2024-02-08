using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using PujcovadloServer.AuthorizationHandlers.Exceptions;
using PujcovadloServer.Business.Entities;
using PujcovadloServer.Business.Enums;
using PujcovadloServer.Business.Services.Interfaces;

namespace PujcovadloServer.AuthorizationHandlers;

public class ItemAuthorizationHandler : BaseCrudAuthorizationHandler<OperationAuthorizationRequirement, Item>
{
    public ItemAuthorizationHandler(IAuthenticateService authenticateService) : base(authenticateService)
    {
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,
        OperationAuthorizationRequirement requirement, Item item)
    {
        await base.HandleRequirementAsync(context, requirement, item);
        
        // Get current user 
        var userId = _authenticateService.TryGetCurrentUserId();

        // TODO refactor spagety
        // Check each action
        switch (requirement.Name)
        {
            case nameof(Operations.Create):
                // Only owners can create a new Item
                if (context.User.IsInRole(UserRoles.Owner))
                    context.Succeed(requirement);
                break;
            case nameof(Operations.Read):
                // Items is public
                if (item.Status == ItemStatus.Public)
                    context.Succeed(requirement);

                // Or I am the owner
                if (userId != null && item.Owner.Id == userId && item.Status != ItemStatus.Deleted)
                    context.Succeed(requirement);

                break;
            case nameof(Operations.Update):
            case nameof(Operations.Delete):
                // I am the owner and the item is not deleted
                if (userId != null && item.Owner.Id == userId && item.Status != ItemStatus.Deleted)
                    context.Succeed(requirement);
                break;
            default:
                throw new UnsupportedOperationException($"Unspported operation {requirement.Name}");
        }
    }
}
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using PujcovadloServer.Business.Enums;
using PujcovadloServer.Business.Services.Interfaces;

namespace PujcovadloServer.AuthorizationHandlers.Item;

public class
    ItemOwnerAuthorizationHandler : AuthorizationHandler<OperationAuthorizationRequirement, Business.Entities.Item>
{
    private readonly IAuthenticateService _authenticateService;

    public ItemOwnerAuthorizationHandler(IAuthenticateService authenticateService)
    {
        _authenticateService = authenticateService;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,
        OperationAuthorizationRequirement requirement,
        Business.Entities.Item resource)
    {
        var userId = _authenticateService.TryGetCurrentUserId();

        // user not authenticated or not owner
        if (userId == null || resource.Owner.Id != userId)
        {
            return;
        }

        // user is owner and can read the item
        if (requirement.Name == ItemOperations.Constants.ReadOperationName)
        {
            context.Succeed(requirement);
        }

        // item is deleted and cannot be updated
        if (resource.Status == ItemStatus.Deleted)
        {
            return;
        }

        // user is owner and can update or delete the item
        if (requirement.Name == ItemOperations.Constants.UpdateOperationName)
        {
            context.Succeed(requirement);
        }

        // user is owner and can delete the item if there are no running rents
        if (requirement.Name == ItemOperations.Constants.DeleteOperationName)
        {
            context.Succeed(requirement);
        }

        // user is owner and can create a new image
        if (requirement.Name == ItemOperations.Constants.CreateImageOperationName)
        {
            context.Succeed(requirement);
        }
    }
}
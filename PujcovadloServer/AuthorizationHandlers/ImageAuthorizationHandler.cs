using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using PujcovadloServer.AuthorizationHandlers.Exceptions;
using PujcovadloServer.Business.Entities;
using PujcovadloServer.Business.Enums;
using PujcovadloServer.Business.Services.Interfaces;

namespace PujcovadloServer.AuthorizationHandlers;

public class ImageAuthorizationHandler : BaseCrudAuthorizationHandler<OperationAuthorizationRequirement, Image>
{
    public ImageAuthorizationHandler(IAuthenticateService authenticateService) : base(authenticateService)
    {
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,
        OperationAuthorizationRequirement requirement, Image image)
    {
        await base.HandleRequirementAsync(context, requirement, image);
        
        var userId = _authenticateService.TryGetCurrentUserId();

        // Check each action
        switch (requirement.Name)
        {
            case nameof(Operations.Create):
                // Only owners of the item can create a new Image
                if(userId != null && image.Item != null && image.Item.Owner.Id == userId)
                    context.Succeed(requirement);
                break;
            case nameof(Operations.Update):
            case nameof(Operations.Delete):
                if (userId != null && image.Owner.Id == userId)
                {
                    context.Succeed(requirement);
                }
                break;
            case nameof(Operations.Read):
                // Anyone can read categories
                context.Succeed(requirement);

                break;
            default:
                throw new UnsupportedOperationException($"Unspported operation {requirement.Name}");
        }
    }
}
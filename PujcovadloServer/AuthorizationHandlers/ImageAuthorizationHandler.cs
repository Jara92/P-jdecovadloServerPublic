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
                /*if (userId != null && image.Item != null && image.Item.Owner.Id == userId)
                    context.Succeed(requirement);*/

                // Only owners of the loan item can create a new Image
                // TODO: add test
                if (userId != null && image.PickupProtocol != null && image.PickupProtocol.Loan.Item.Owner.Id == userId)
                    context.Succeed(requirement);

                // Only owners of the loan item can create a new Image
                // TODO: add test
                if (userId != null && image.ReturnProtocol != null && image.ReturnProtocol.Loan.Item.Owner.Id == userId)
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
                // Owner can read the image
                /*if (userId != null && image.Owner.Id == userId)
                {
                    context.Succeed(requirement);
                }*/

                // Is items owner and the item is not deleted
                if (image.Item != null && image.Item.Owner.Id == userId && image.Item.Status != ItemStatus.Deleted)
                {
                    context.Succeed(requirement);
                }

                // Image's item is public
                if (image.Item != null && image.Item.Status == ItemStatus.Public)
                {
                    context.Succeed(requirement);
                }

                // Image has a PickupProtocol and the user is the tenant or the owner
                if (image.PickupProtocol != null && (image.PickupProtocol.Loan.Tenant.Id == userId ||
                                                     image.PickupProtocol.Loan.Item.Owner.Id == userId))
                {
                    context.Succeed(requirement);
                }

                // Image has a ReturnProtocol and the user is the tenant or the owner
                if (image.ReturnProtocol != null && (image.ReturnProtocol.Loan.Tenant.Id == userId ||
                                                     image.ReturnProtocol.Loan.Item.Owner.Id == userId))
                {
                    context.Succeed(requirement);
                }

                break;
            default:
                throw new UnsupportedOperationException($"Unspported operation {requirement.Name}");
        }
    }
}
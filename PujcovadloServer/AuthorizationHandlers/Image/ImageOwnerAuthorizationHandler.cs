using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using PujcovadloServer.Business.Services.Interfaces;

namespace PujcovadloServer.AuthorizationHandlers.Image;

public class
    ImageOwnerAuthorizationHandler : AuthorizationHandler<OperationAuthorizationRequirement, Business.Entities.Image>
{
    private readonly IAuthenticateService _authenticateService;

    public ImageOwnerAuthorizationHandler(IAuthenticateService authenticateService)
    {
        _authenticateService = authenticateService;
    }

    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
        OperationAuthorizationRequirement requirement,
        Business.Entities.Image resource)
    {
        var userId = _authenticateService.TryGetCurrentUserId();

        // user not authenticated or not owner
        if (userId == null || resource.Owner.Id != userId)
        {
            return Task.CompletedTask;
        }

        // user is owner and can read the image
        if (requirement.Name == ImageOperations.Constants.ReadOperationName)
        {
            context.Succeed(requirement);
        }

        // user is owner and can delete the image
        if (requirement.Name == ImageOperations.Constants.DeleteOperationName)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
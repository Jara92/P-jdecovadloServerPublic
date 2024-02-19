using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using PujcovadloServer.Business.Services.Interfaces;

namespace PujcovadloServer.AuthorizationHandlers.Image;

public class
    ImageGuestAuthorizationHandler : AuthorizationHandler<OperationAuthorizationRequirement, Business.Entities.Image>
{
    private readonly IAuthenticateService _authenticateService;

    public ImageGuestAuthorizationHandler(IAuthenticateService authenticateService)
    {
        _authenticateService = authenticateService;
    }

    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
        OperationAuthorizationRequirement requirement,
        Business.Entities.Image resource)
    {
        // Guest can only read images
        if (requirement.Name != ImageOperations.Constants.ReadOperationName)
        {
            return Task.CompletedTask;
        }

        // resource belongs to a public item
        if (resource.Item != null && resource.Item.Status == Business.Enums.ItemStatus.Public)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
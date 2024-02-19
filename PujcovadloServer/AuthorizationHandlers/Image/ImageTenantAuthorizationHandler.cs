using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using PujcovadloServer.Business.Services.Interfaces;

namespace PujcovadloServer.AuthorizationHandlers.Image;

public class
    ImageTenantAuthorizationHandler : AuthorizationHandler<OperationAuthorizationRequirement, Business.Entities.Image>
{
    private readonly IAuthenticateService _authenticateService;

    public ImageTenantAuthorizationHandler(IAuthenticateService authenticateService)
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

        var userId = _authenticateService.TryGetCurrentUserId();

        // image belongs to a pickup protocol and user is tenant of the loan
        if (resource.PickupProtocol != null && resource.PickupProtocol.Loan.Tenant.Id == userId)
        {
            context.Succeed(requirement);
        }

        // image belongs to a return protocol and user is tenant of the loan
        if (resource.ReturnProtocol != null && resource.ReturnProtocol.Loan.Tenant.Id == userId)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using PujcovadloServer.Business.Enums;
using PujcovadloServer.Business.Services.Interfaces;

namespace PujcovadloServer.AuthorizationHandlers.PickupProtocol;

public class PickupProtocolAdminAuthorizationHandler : AuthorizationHandler<OperationAuthorizationRequirement,
    Business.Entities.PickupProtocol>
{
    private readonly IAuthenticateService _authenticateService;

    public PickupProtocolAdminAuthorizationHandler(IAuthenticateService authenticateService)
    {
        _authenticateService = authenticateService;
    }

    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
        OperationAuthorizationRequirement requirement,
        Business.Entities.PickupProtocol resource)
    {
        // admin can do anything
        if (context.User.IsInRole(UserRoles.Admin))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
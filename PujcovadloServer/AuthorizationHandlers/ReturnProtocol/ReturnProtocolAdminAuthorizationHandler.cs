using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using PujcovadloServer.Business.Enums;
using PujcovadloServer.Business.Services.Interfaces;

namespace PujcovadloServer.AuthorizationHandlers.ReturnProtocol;

public class ReturnProtocolAdminAuthorizationHandler : AuthorizationHandler<OperationAuthorizationRequirement,
    Business.Entities.ReturnProtocol>
{
    private readonly IAuthenticateService _authenticateService;

    public ReturnProtocolAdminAuthorizationHandler(IAuthenticateService authenticateService)
    {
        _authenticateService = authenticateService;
    }

    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
        OperationAuthorizationRequirement requirement,
        Business.Entities.ReturnProtocol resource)
    {
        // admin can do anything
        if (context.User.IsInRole(UserRoles.Admin))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
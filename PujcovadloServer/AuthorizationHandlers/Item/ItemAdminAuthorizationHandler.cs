using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using PujcovadloServer.Business.Enums;
using PujcovadloServer.Business.Services.Interfaces;

namespace PujcovadloServer.AuthorizationHandlers.Item;

public class
    ItemAdminAuthorizationHandler : AuthorizationHandler<OperationAuthorizationRequirement, Business.Entities.Item>
{
    private readonly IAuthenticateService _authenticateService;

    public ItemAdminAuthorizationHandler(IAuthenticateService authenticateService)
    {
        _authenticateService = authenticateService;
    }

    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
        OperationAuthorizationRequirement requirement,
        Business.Entities.Item resource)
    {
        // Admin can do anything
        if (context.User.IsInRole(UserRoles.Admin))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
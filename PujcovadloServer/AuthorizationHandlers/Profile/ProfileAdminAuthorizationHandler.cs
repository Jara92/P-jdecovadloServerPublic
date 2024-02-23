using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using PujcovadloServer.Business.Enums;
using PujcovadloServer.Business.Services.Interfaces;

namespace PujcovadloServer.AuthorizationHandlers.Profile;

public class
    ProfileAdminAuthorizationHandler : AuthorizationHandler<OperationAuthorizationRequirement,
    Business.Entities.Profile>
{
    private readonly IAuthenticateService _authenticateService;

    public ProfileAdminAuthorizationHandler(IAuthenticateService authenticateService)
    {
        _authenticateService = authenticateService;
    }

    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
        OperationAuthorizationRequirement requirement,
        Business.Entities.Profile resource)
    {
        // admin can do anything
        if (context.User.IsInRole(UserRoles.Admin))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using PujcovadloServer.Business.Enums;
using PujcovadloServer.Business.Services.Interfaces;

namespace PujcovadloServer.AuthorizationHandlers.Review;

public class ReviewAdminAuthorizationHandler : AuthorizationHandler<OperationAuthorizationRequirement,
    Business.Entities.Review>
{
    private readonly IAuthenticateService _authenticateService;

    public ReviewAdminAuthorizationHandler(IAuthenticateService authenticateService)
    {
        _authenticateService = authenticateService;
    }

    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
        OperationAuthorizationRequirement requirement,
        Business.Entities.Review resource)
    {
        // admin can do anything
        if (context.User.IsInRole(UserRoles.Admin))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
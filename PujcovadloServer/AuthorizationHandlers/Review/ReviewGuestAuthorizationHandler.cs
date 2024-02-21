using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using PujcovadloServer.AuthorizationHandlers.ReturnProtocol;
using PujcovadloServer.Business.Services.Interfaces;

namespace PujcovadloServer.AuthorizationHandlers.Review;

public class
    ReviewGuestAuthorizationHandler : AuthorizationHandler<OperationAuthorizationRequirement, Business.Entities.Review>
{
    private readonly IAuthenticateService _authenticateService;

    public ReviewGuestAuthorizationHandler(IAuthenticateService authenticateService)
    {
        _authenticateService = authenticateService;
    }

    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
        OperationAuthorizationRequirement requirement,
        Business.Entities.Review resource)
    {
        // Anyone can read a review
        if (requirement.Name == ReviewOperations.Constants.ReadOperationName)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
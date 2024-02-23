using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using PujcovadloServer.Business.Services.Interfaces;

namespace PujcovadloServer.AuthorizationHandlers.Profile;

public class
    ProfileGuestAuthorizationHandler : AuthorizationHandler<OperationAuthorizationRequirement,
    Business.Entities.Profile>
{
    private readonly IAuthenticateService _authenticateService;

    public ProfileGuestAuthorizationHandler(IAuthenticateService authenticateService)
    {
        _authenticateService = authenticateService;
    }

    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
        OperationAuthorizationRequirement requirement,
        Business.Entities.Profile resource)
    {
        // Anyone can read profile
        if (requirement.Name == ProfileOperations.Constants.ReadOperationName)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
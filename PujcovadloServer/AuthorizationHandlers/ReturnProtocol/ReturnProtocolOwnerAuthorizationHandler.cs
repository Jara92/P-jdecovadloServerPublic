using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using PujcovadloServer.Business.Services.Interfaces;

namespace PujcovadloServer.AuthorizationHandlers.ReturnProtocol;

public class ReturnProtocolOwnerAuthorizationHandler : AuthorizationHandler<OperationAuthorizationRequirement,
    Business.Entities.ReturnProtocol>
{
    private readonly IAuthenticateService _authenticateService;

    public ReturnProtocolOwnerAuthorizationHandler(IAuthenticateService authenticateService)
    {
        _authenticateService = authenticateService;
    }

    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
        OperationAuthorizationRequirement requirement,
        Business.Entities.ReturnProtocol resource)
    {
        var userId = _authenticateService.TryGetCurrentUserId();

        // Fail if user is not authenticated or not the owner
        if (userId == null || resource.Loan.Item.Owner.Id != userId)
        {
            return Task.CompletedTask;
        }

        // Owner can read his loans
        if (requirement.Name == ReturnProtocolOperations.Constants.ReadOperationName)
        {
            context.Succeed(requirement);
        }

        // Owner can update his loans
        if (requirement.Name == ReturnProtocolOperations.Constants.UpdateOperationName)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
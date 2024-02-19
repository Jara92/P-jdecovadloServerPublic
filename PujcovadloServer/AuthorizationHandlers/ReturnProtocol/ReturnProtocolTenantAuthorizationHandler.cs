using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using PujcovadloServer.Business.Services.Interfaces;

namespace PujcovadloServer.AuthorizationHandlers.ReturnProtocol;

public class ReturnProtocolTenantAuthorizationHandler : AuthorizationHandler<OperationAuthorizationRequirement,
    Business.Entities.ReturnProtocol>
{
    private readonly IAuthenticateService _authenticateService;

    public ReturnProtocolTenantAuthorizationHandler(IAuthenticateService authenticateService)
    {
        _authenticateService = authenticateService;
    }

    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
        OperationAuthorizationRequirement requirement,
        Business.Entities.ReturnProtocol resource)
    {
        var userId = _authenticateService.TryGetCurrentUserId();

        // Fail if user is not authenticated or not the tenant
        if (userId == null || resource.Loan.Tenant.Id != userId)
        {
            return Task.CompletedTask;
        }

        // Owner can read his loans
        if (requirement.Name == ReturnProtocolOperations.Constants.ReadOperationName)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
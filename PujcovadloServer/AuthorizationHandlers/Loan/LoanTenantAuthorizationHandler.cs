using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using PujcovadloServer.Business.Enums;
using PujcovadloServer.Business.Services.Interfaces;

namespace PujcovadloServer.AuthorizationHandlers.Loan;

public class
    LoanTenantAuthorizationHandler : AuthorizationHandler<OperationAuthorizationRequirement, Business.Entities.Loan>
{
    private readonly IAuthenticateService _authenticateService;

    public LoanTenantAuthorizationHandler(IAuthenticateService authenticateService)
    {
        _authenticateService = authenticateService;
    }

    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
        OperationAuthorizationRequirement requirement,
        Business.Entities.Loan resource)
    {
        // user must be in tenant role
        if (!context.User.IsInRole(UserRoles.Tenant))
        {
            return Task.CompletedTask;
        }

        // Tenant can create new loans
        if (requirement.Name == LoanOperations.Constants.CreateOperationName)
        {
            context.Succeed(requirement);
        }

        // Get current user id
        var userId = _authenticateService.TryGetCurrentUserId();

        // Fail if user is not authenticated or not the tenant of the loan
        if (userId == null || resource.Tenant == null || resource.Tenant.Id != userId)
        {
            return Task.CompletedTask;
        }

        // Tenant can read his loans
        if (requirement.Name == LoanOperations.Constants.ReadOperationName)
        {
            context.Succeed(requirement);
        }

        // Tenant can update his loans
        if (requirement.Name == LoanOperations.Constants.UpdateOperationName)
        {
            context.Succeed(requirement);
        }

        // tenant can review the loan
        if (requirement.Name == LoanOperations.Constants.ReviewOperationName)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
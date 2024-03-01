using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using PujcovadloServer.Business.Services.Interfaces;

namespace PujcovadloServer.AuthorizationHandlers.Loan;

public class
    LoanOwnerAuthorizationHandler : AuthorizationHandler<OperationAuthorizationRequirement, Business.Entities.Loan>
{
    private readonly IAuthenticateService _authenticateService;

    public LoanOwnerAuthorizationHandler(IAuthenticateService authenticateService)
    {
        _authenticateService = authenticateService;
    }

    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
        OperationAuthorizationRequirement requirement,
        Business.Entities.Loan resource)
    {
        var userId = _authenticateService.TryGetCurrentUserId();

        // Fail if user is not authenticated or not the owner
        if (userId == null || resource.Item == null || resource.Item.Owner.Id != userId)
        {
            return Task.CompletedTask;
        }

        // Owner can read his loans
        if (requirement.Name == LoanOperations.Constants.ReadOperationName)
        {
            context.Succeed(requirement);
        }

        // Owner can update his loans
        if (requirement.Name == LoanOperations.Constants.UpdateOperationName)
        {
            context.Succeed(requirement);
        }

        // owner can create protocols for his loans
        if (requirement.Name == LoanOperations.Constants.CreatePickupProtocolOperationName ||
            requirement.Name == LoanOperations.Constants.CreateReturnProtocolOperationName)
        {
            context.Succeed(requirement);
        }

        // owner can review the loan
        if (requirement.Name == LoanOperations.Constants.ReviewOperationName)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
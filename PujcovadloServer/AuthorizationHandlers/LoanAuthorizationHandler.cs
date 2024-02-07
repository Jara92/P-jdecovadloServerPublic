using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Namotion.Reflection;
using PujcovadloServer.AuthorizationHandlers.Exceptions;
using PujcovadloServer.Business.Entities;
using PujcovadloServer.Business.Enums;
using PujcovadloServer.Business.Services.Interfaces;

namespace PujcovadloServer.AuthorizationHandlers;

public class LoanAuthorizationHandler : BaseCrudAuthorizationHandler<OperationAuthorizationRequirement, Loan>
{
    public LoanAuthorizationHandler(IAuthenticateService authenticateService) : base(authenticateService)
    {
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,
        OperationAuthorizationRequirement requirement, Loan loan)
    {
        await base.HandleRequirementAsync(context, requirement, loan);

        // Get current user id
        var userId = _authenticateService.GetCurrentUserId();

        // TODO refactor spagety
        // Check each action
        switch (requirement.Name)
        {
            case nameof(Operations.Create):
                // Only owners can create a new Item
                if (context.User.IsInRole(UserRoles.Tenant))
                    context.Succeed(requirement);
                break;
            case nameof(Operations.Read):
                // User is the tenant or the owner
                if (loan.Tenant.Id == userId || loan.Item.Owner.Id == userId)
                    context.Succeed(requirement);

                break;
            case nameof(Operations.Update):
            case nameof(Operations.Delete):
                // I am the owner
                if (loan.Tenant.Id == userId || loan.Item.Owner.Id == userId)
                    context.Succeed(requirement);
                break;
            case nameof(Operations.CreatePickupProtocol):
                // Only the owner can create the protocol and the loan must be accepted
                if (loan.Item.Owner.Id == userId
                    && loan.Status == LoanStatus.Accepted
                    && loan.PickupProtocol == null)
                {
                    context.Succeed(requirement);
                }

                break;
            default:
                throw new UnsupportedOperationException($"Unspported operation {requirement.Name}");
        }
    }

    public static class Operations
    {
        public static OperationAuthorizationRequirement Create = new OperationAuthorizationRequirement
            { Name = nameof(Create) };

        public static OperationAuthorizationRequirement Read = new OperationAuthorizationRequirement
            { Name = nameof(Read) };

        public static OperationAuthorizationRequirement Update = new OperationAuthorizationRequirement
            { Name = nameof(Update) };

        public static OperationAuthorizationRequirement Delete = new OperationAuthorizationRequirement
            { Name = nameof(Delete) };

        public static OperationAuthorizationRequirement CreatePickupProtocol = new OperationAuthorizationRequirement
            { Name = nameof(CreatePickupProtocol) };
    }
}
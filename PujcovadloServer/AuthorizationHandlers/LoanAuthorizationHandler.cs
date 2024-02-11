using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
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
        var userId = _authenticateService.TryGetCurrentUserId();

        // If the user is not authenticated, return
        if (userId == null)
            return;

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
                // User is the tenant or the owner
                if (loan.Tenant.Id == userId || loan.Item.Owner.Id == userId)
                    context.Succeed(requirement);
                break;
            case nameof(Operations.Delete):
                // Noone can do this
                break;
            case nameof(Operations.CreatePickupProtocol):
                // Only the owner can create the protocol
                if (loan.Item.Owner.Id == userId)
                {
                    context.Succeed(requirement);
                }

                break;
            case nameof(Operations.IsOwner):
                // I am the owner
                // TODO: check options for reusing the this requirement inside other requirements
                if (loan.Item.Owner.Id == userId)
                    context.Succeed(requirement);
                break;
            case nameof(Operations.IsTenant):
                // I am the tenant
                // TODO: check options for reusing the this requirement inside other requirements
                if (loan.Tenant.Id == userId)
                    context.Succeed(requirement);
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

        /*public static OperationAuthorizationRequirement CreateReturnProtocol = new OperationAuthorizationRequirement
            { Name = nameof(CreateReturnProtocol) };*/

        public static OperationAuthorizationRequirement IsOwner = new OperationAuthorizationRequirement
            { Name = nameof(IsOwner) };

        public static OperationAuthorizationRequirement IsTenant = new OperationAuthorizationRequirement
            { Name = nameof(IsTenant) };
    }
}
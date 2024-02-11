using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using PujcovadloServer.AuthorizationHandlers.Exceptions;
using PujcovadloServer.Business.Entities;
using PujcovadloServer.Business.Services.Interfaces;

namespace PujcovadloServer.AuthorizationHandlers;

public class
    PickupProtocolAuthorizationHandler : BaseCrudAuthorizationHandler<OperationAuthorizationRequirement, PickupProtocol>
{
    public PickupProtocolAuthorizationHandler(IAuthenticateService authenticateService) : base(authenticateService)
    {
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,
        OperationAuthorizationRequirement requirement, PickupProtocol protocol)
    {
        await base.HandleRequirementAsync(context, requirement, protocol);

        // Get current user id
        var userId = _authenticateService.GetCurrentUserId();

        // TODO refactor spagety
        // Check each action
        switch (requirement.Name)
        {
            case nameof(Operations.Create):
                // todo: find a way how to delete this requirement to other AuthorizationHandler
                if (protocol.Loan.Item.Owner.Id == userId)
                {
                    context.Succeed(requirement);
                }

                break;
            case nameof(Operations.Read):
                // User is the tenant or the owner
                if (protocol.Loan.Tenant.Id == userId || protocol.Loan.Item.Owner.Id == userId)
                    context.Succeed(requirement);
                break;
            case nameof(Operations.Update):
                // Only the owner can update the protocol but the loan pickup must be denied
                if (protocol.Loan.Item.Owner.Id == userId)
                    context.Succeed(requirement);

                break;

            case nameof(Operations.Delete):
                // No one can delete the protocol
                break;
            default:
                throw new UnsupportedOperationException($"Unspported operation {requirement.Name}");
        }
    }
}
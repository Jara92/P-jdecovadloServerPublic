using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using PujcovadloServer.AuthorizationHandlers.Exceptions;
using PujcovadloServer.Business.Entities;
using PujcovadloServer.Business.Enums;
using PujcovadloServer.Business.Services.Interfaces;

namespace PujcovadloServer.AuthorizationHandlers;

public class
    PickupProtocolAuthorizationHandler : BaseCrudAuthorizationHandler<OperationAuthorizationRequirement, PickupProtocol>
{
    private readonly IAuthorizationService _authorizationService;
    
    public PickupProtocolAuthorizationHandler(IAuthenticateService authenticateService, IAuthorizationService authorizationService) : base(authenticateService)
    {
        _authorizationService = authorizationService;
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
                // Delegate the problem to LoanAuthorizationHandler
                var result = await _authorizationService.AuthorizeAsync(context.User, protocol.Loan, LoanAuthorizationHandler.Operations.CreatePickupProtocol);
                
                if (result.Succeeded)
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
                if (protocol.Loan.Item.Owner.Id == userId && protocol.Loan.Status == LoanStatus.PickupDenied)
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
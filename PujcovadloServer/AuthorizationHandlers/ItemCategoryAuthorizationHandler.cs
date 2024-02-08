using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using PujcovadloServer.AuthorizationHandlers.Exceptions;
using PujcovadloServer.Business.Entities;
using PujcovadloServer.Business.Enums;
using PujcovadloServer.Business.Services.Interfaces;

namespace PujcovadloServer.AuthorizationHandlers;

public class
    ItemCategoryAuthorizationHandler : BaseCrudAuthorizationHandler<OperationAuthorizationRequirement, ItemCategory>
{
    public ItemCategoryAuthorizationHandler(IAuthenticateService authenticateService) : base(authenticateService)
    {
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,
        OperationAuthorizationRequirement requirement, ItemCategory loan)
    {
        await base.HandleRequirementAsync(context, requirement, loan);
        
        // Check each action
        switch (requirement.Name)
        {
            case nameof(Operations.Create):
            case nameof(Operations.Update):
            case nameof(Operations.Delete):
                // No one can create, update or delete categories
                break;
            case nameof(Operations.Read):
                // Anyone can read categories
                context.Succeed(requirement);

                break;
            default:
                throw new UnsupportedOperationException($"Unspported operation {requirement.Name}");
        }
    }
}
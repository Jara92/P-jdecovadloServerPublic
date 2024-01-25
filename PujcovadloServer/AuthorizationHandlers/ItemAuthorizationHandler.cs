using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using PujcovadloServer.AuthorizationHandlers.Exceptions;
using PujcovadloServer.Business.Entities;
using PujcovadloServer.Business.Enums;
using PujcovadloServer.Business.Services.Interfaces;

namespace PujcovadloServer.AuthorizationHandlers;

public class ItemAuthorizationHandler : AuthorizationHandler<OperationAuthorizationRequirement, Item>
{
    private readonly IAuthenticateService _authenticateService;

    public ItemAuthorizationHandler(IAuthenticateService authenticateService)
    {
        _authenticateService = authenticateService;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,
        OperationAuthorizationRequirement requirement,
        Item item)
    {
        // Get current user 
        var user = await _authenticateService.GetCurrentUser();

        if (user == null)
            return;

        // TODO refactor spagety
        // Check each action
        switch (requirement.Name)
        {
            case nameof(Operations.Create):
                // Only owners can create a new Item
                if (context.User.IsInRole(UserRoles.Owner))
                    context.Succeed(requirement);
                break;
            case nameof(Operations.Read):
                // Items is public
                if (item.Status == ItemStatus.Public)
                    context.Succeed(requirement);

                // Or I am the owner
                if (item.Owner.Id == user.Id)
                    context.Succeed(requirement);

                break;
            case nameof(Operations.Update):
            case nameof(Operations.Delete):
                // I am the owner
                if (item.Owner.Id == user.Id)
                    context.Succeed(requirement);
                break;
            default:
                throw new UnsupportedOperationException($"Unspported operation {requirement.Name}");
        }
    }

    /// <summary>
    /// Available operations to check access for.
    /// </summary>
    public static class Operations
    {
        public static OperationAuthorizationRequirement Create =
            new OperationAuthorizationRequirement { Name = nameof(Create) };

        public static OperationAuthorizationRequirement Read =
            new OperationAuthorizationRequirement { Name = nameof(Read) };

        public static OperationAuthorizationRequirement Update =
            new OperationAuthorizationRequirement { Name = nameof(Update) };

        public static OperationAuthorizationRequirement Delete =
            new OperationAuthorizationRequirement { Name = nameof(Delete) };
    }
}
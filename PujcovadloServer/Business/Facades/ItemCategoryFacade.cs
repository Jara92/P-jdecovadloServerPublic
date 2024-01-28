using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using PujcovadloServer.AuthorizationHandlers;
using PujcovadloServer.AuthorizationHandlers.Exceptions;
using PujcovadloServer.Business.Entities;
using PujcovadloServer.Business.Exceptions;
using PujcovadloServer.Business.Services;
using PujcovadloServer.Business.Services.Interfaces;
using PujcovadloServer.Helpers;

namespace PujcovadloServer.Business.Facades;

public class ItemCategoryFacade
{
    private readonly ItemCategoryService _itemCategoryService;
    private readonly IAuthenticateService _authenticateService;
    private readonly IAuthorizationService _authorizationService;

    public ItemCategoryFacade(ItemCategoryService itemCategoryService, IAuthenticateService authenticateService, 
        IAuthorizationService authorizationService)
    {
        _itemCategoryService = itemCategoryService;
        _authenticateService = authenticateService;
        _authorizationService = authorizationService;
    }

    /// <summary>
    /// Returns category by given id.
    /// </summary>
    /// <param name="id">Category id</param>
    /// <returns>Category identified by given id</returns>
    /// <exception cref="EntityNotFoundException">Thrown if category with the id does not exist.</exception>
    public async Task<ItemCategory> Get(int id)
    {
        var category =  await _itemCategoryService.Get(id);

        if (category == null) throw new EntityNotFoundException($"ItemCategory with {id} not found.");
        
        await CheckPermissions(category, ItemCategoryAuthorizationHandler.Operations.Read);

        return category;
    }

    public async Task Create(ItemCategory newCategory)
    {
        await CheckPermissions(newCategory, ItemCategoryAuthorizationHandler.Operations.Create);
        
        newCategory.Alias = UrlHelper.CreateUrlStub(newCategory.Name);
        
        await _itemCategoryService.Create(newCategory);
    }

    public async Task Update(ItemCategory category)
    {
        await CheckPermissions(category, ItemCategoryAuthorizationHandler.Operations.Update);
        
        category.Alias = UrlHelper.CreateUrlStub(category.Name);
        
        await _itemCategoryService.Update(category);
    }
    
    /// <summary>
    /// Checks if the user has permissions to perform the operation on the item.
    /// </summary>
    /// <param name="category">The item.</param>
    /// <param name="requirement">Required action</param>
    /// <exception cref="ForbiddenAccessException">User does not have permission to perform the action.</exception>
    /// <exception cref="UnauthorizedAccessException">User is not authorized.</exception>
    private async Task CheckPermissions(ItemCategory category, OperationAuthorizationRequirement requirement)
    {
        // Get current principal
        var principal = _authenticateService.GetPrincipal();
        if(principal == null) throw new UnauthorizedAccessException();
        
        // Check requirement permissions
        var authorizationResult = await _authorizationService.AuthorizeAsync(
            principal, category, requirement);

        // Throw exception if not authorized
        if (!authorizationResult.Succeeded)
        {
            var identity = principal.Identity;
         
            // Throw UnauthorizedAccessException if not authenticated
            if (identity == null || !identity.IsAuthenticated)
                throw new UnauthorizedAccessException();
            
            // Throw ForbiddenAccessException if not authorized
            throw new ForbiddenAccessException("You are not authorized to perform this operation.");
        }
    }
}
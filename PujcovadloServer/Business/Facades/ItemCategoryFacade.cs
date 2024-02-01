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

        return category;
    }

    public async Task Create(ItemCategory newCategory)
    {
        newCategory.Alias = UrlHelper.CreateUrlStub(newCategory.Name);
        
        await _itemCategoryService.Create(newCategory);
    }

    public async Task Update(ItemCategory category)
    {
        category.Alias = UrlHelper.CreateUrlStub(category.Name);
        
        await _itemCategoryService.Update(category);
    }
}
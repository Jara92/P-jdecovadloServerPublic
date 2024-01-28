using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using NuGet.Packaging;
using PujcovadloServer.Authentication;
using PujcovadloServer.AuthorizationHandlers;
using PujcovadloServer.AuthorizationHandlers.Exceptions;
using PujcovadloServer.Business.Entities;
using PujcovadloServer.Business.Enums;
using PujcovadloServer.Business.Exceptions;
using PujcovadloServer.Business.Filters;
using PujcovadloServer.Business.Interfaces;
using PujcovadloServer.Business.Services;
using PujcovadloServer.Business.Services.Interfaces;
using PujcovadloServer.Helpers;
using PujcovadloServer.Lib;
using PujcovadloServer.Requests;

namespace PujcovadloServer.Business.Facades;

public class ItemFacade
{
    private readonly IItemRepository _itemRepository;
    private readonly ItemService _itemService;
    private readonly ItemCategoryService _itemCategoryService;
    private readonly IAuthenticateService _authenticateService;
    private readonly IMapper _mapper;
    private readonly IAuthorizationService _authorizationService;

    public ItemFacade(IItemRepository itemRepository, ItemService itemService, ItemCategoryService itemCategoryService,
        IAuthenticateService authenticateService, IMapper mapper, IAuthorizationService authorizationService)
    {
        _itemRepository = itemRepository;
        _itemService = itemService;
        _itemCategoryService = itemCategoryService;
        _authenticateService = authenticateService;
        _mapper = mapper;
        _authorizationService = authorizationService;
    }

    /// <summary>
    /// Creates a new item using <see cref="ItemRequest"/>
    /// </summary>
    /// <param name="request"></param>
    public async Task<Item> CreateItem(ItemRequest request)
    {
        var user = await _authenticateService.GetCurrentUser();
        if (user == null) throw new UnauthorizedAccessException("User not found.");

        // Map request to item
        var item = _mapper.Map<Item>(request);

        // Set initial status
        item.Status = ItemStatus.Public;

        // Set alias
        item.Alias = UrlHelper.CreateUrlStub(item.Name);

        // Set owner
        item.Owner = user;
        
        // Check permissions
        await CheckPermissions(item, ItemAuthorizationHandler.Operations.Create);

        // Create the item  
        await _itemService.Create(item);

        return item;
    }

    public async Task UpdateItem(ItemRequest request)
    {
        // Updated tracked item
        var item = await _itemRepository.Get(request.Id);

        if (item == null) throw new EntityNotFoundException($"Item with id {request.Id} not found.");

        // Check authorization
        await CheckPermissions(item, ItemAuthorizationHandler.Operations.Update);

        // Map request to item
        item.Name = request.Name;
        item.Alias = UrlHelper.CreateUrlStub(item.Name);
        item.Description = request.Description;

        // update prices
        item.PricePerDay = request.PricePerDay;
        item.PurchasePrice = request.PurchasePrice;
        item.SellingPrice = request.SellingPrice;

        // New categories
        var ids = request.Categories.Select(c => c.Id);
        var categories = await _itemCategoryService.GetByIds(ids);
        
        // Update categories
        item.Categories.Clear();
        item.Categories.AddRange(categories);

        // Item updated so we need to approve it
        if (item.Status == ItemStatus.Denied)
            item.Status = ItemStatus.Approving;

        // Todo: update images

        // Update the item
        // 
        await _itemService.Update(item);
    }

    public async Task DeleteItem(int id)
    {
        // Get the item
        var item = await _itemService.Get(id);
        if (item == null) throw new EntityNotFoundException($"Item with id {id} not found.");
        
        // Check permissions
        await CheckPermissions(item, ItemAuthorizationHandler.Operations.Delete);
        
        // TODO: Check that the item can be deleted (no active rentals etc.)

        // Delete the item
        await _itemService.Delete(item);
    }

    public async Task<PaginatedList<Item>> GetMyItems(ItemFilter filter)
    {
        var user = await _authenticateService.GetCurrentUser();

        if (user == null) throw new UnauthorizedAccessException("User not found.");

        // Set owner id
        filter.OwnerId = user.Id;

        // Get items
        var items = await _itemService.GetAll(filter);

        return items;
    }
    
    /// <summary>
    /// Returns item with given id.
    /// </summary>
    /// <param name="id">Item's id</param>
    /// <returns>Item</returns>
    /// <exception cref="EntityNotFoundException">Thrown when item id is invalid.</exception>
    public async Task<Item> GetItem(int id)
    {
        var item = await _itemService.Get(id);
        
        // Check if item exists
        if (item == null) throw new EntityNotFoundException($"Item with id {id} not found.");
        
        // Check permissions
        await CheckPermissions(item, ItemAuthorizationHandler.Operations.Read);
        
        // Return item
        return item;
    }
    
    /// <summary>
    /// Returns item with given id if it belongs to the user.
    /// </summary>
    /// <param name="id">Id of the item</param>
    /// <returns>The item.</returns>
    /// <exception cref="ForbiddenAccessException">Throw if the user is not the owner</exception>
    public async Task<Item> GetMyItem(int id)
    {
        var item = await GetItem(id);
        
        // Check if item belongs to the user
        if(item.Owner.Id != (await _authenticateService.GetCurrentUser()).Id)
            throw new ForbiddenAccessException("You are not authorized to perform this operation.");
        
        return item;
    }

    /// <summary>
    /// Checks if the user has permissions to perform the operation on the item.
    /// </summary>
    /// <param name="item">The item.</param>
    /// <param name="requirement">Required action</param>
    /// <exception cref="ForbiddenAccessException">User does not have permission to perform the action.</exception>
    /// <exception cref="UnauthorizedAccessException">User is not authorized.</exception>
    private async Task CheckPermissions(Item item, OperationAuthorizationRequirement requirement)
    {
        // Get current principal
        var principal = _authenticateService.GetPrincipal();
        if(principal == null) throw new UnauthorizedAccessException();
        
        // Check requirement permissions
        var authorizationResult = await _authorizationService.AuthorizeAsync(
            principal, item, requirement);

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
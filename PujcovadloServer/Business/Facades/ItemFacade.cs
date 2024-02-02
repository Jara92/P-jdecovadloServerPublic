using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using NuGet.Packaging;
using PujcovadloServer.Authentication.Exceptions;
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
    private readonly ItemTagService _itemTagService;
    private readonly IAuthenticateService _authenticateService;
    private readonly IMapper _mapper;
    private readonly IAuthorizationService _authorizationService;

    public ItemFacade(IItemRepository itemRepository, ItemService itemService, ItemCategoryService itemCategoryService,
        ItemTagService itemTagService, IAuthenticateService authenticateService, IMapper mapper, IAuthorizationService authorizationService)
    {
        _itemRepository = itemRepository;
        _itemService = itemService;
        _itemCategoryService = itemCategoryService;
        _itemTagService = itemTagService;
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
        if (user == null) throw new NotAuthenticatedException("User not found.");

        // Map request to item
        // Todo: must be done manually because of categories and tags
        var item = _mapper.Map<Item>(request);

        // Set initial status
        item.Status = ItemStatus.Public;

        // Set alias
        item.Alias = UrlHelper.CreateUrlStub(item.Name);

        // Set owner
        item.Owner = user;

        // Create the item  
        await _itemService.Create(item);

        return item;
    }

    public async Task UpdateItem(Item item, ItemRequest request)
    {
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
        
        // new tags
        var tags = await _itemTagService.GetOrCreate(request.Tags.Select(t => t.Name).ToList());

        // Update tags
        item.Tags.Clear();
        item.Tags.AddRange(tags);
        
        // Todo: update images

        // Update the item
        // 
        await _itemService.Update(item);
    }

    public async Task DeleteItem(Item item)
    {
        // TODO: Check that the item can be deleted (no active rentals etc.)

        // Delete the item
        await _itemService.Delete(item);
    }

    public async Task<PaginatedList<Item>> GetMyItems(ItemFilter filter)
    {
        var user = await _authenticateService.GetCurrentUser();

        if (user == null) throw new NotAuthenticatedException("User not found.");

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

        // Return item
        return item;
    }
}
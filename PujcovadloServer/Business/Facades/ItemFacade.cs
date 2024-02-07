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
    private readonly ItemService _itemService;
    private readonly LoanService _loanService;
    private readonly ItemCategoryService _itemCategoryService;
    private readonly ItemTagService _itemTagService;
    private readonly IAuthenticateService _authenticateService;
    private readonly IMapper _mapper;
    private readonly PujcovadloServerConfiguration _configuration;

    public ItemFacade(ItemService itemService, LoanService loanService, ItemCategoryService itemCategoryService,
        ItemTagService itemTagService, IAuthenticateService authenticateService, IMapper mapper,
        PujcovadloServerConfiguration configuration)
    {
        _itemService = itemService;
        _loanService = loanService;
        _itemCategoryService = itemCategoryService;
        _itemTagService = itemTagService;
        _authenticateService = authenticateService;
        _mapper = mapper;
        _configuration = configuration;
    }

    /// <summary>
    /// Fill request data to the item.
    /// </summary>
    /// <param name="item">The item.</param>
    /// <param name="request">Item request data.</param>
    private async Task FillItemRequest(Item item, ItemRequest request)
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
        
        // new tags
        var tags = await _itemTagService.GetOrCreate(request.Tags.Select(t => t.Name).ToList());

        // Update tags
        item.Tags.Clear();
        item.Tags.AddRange(tags);

        // Todo: update images
    }

    /// <summary>
    /// Creates a new item using <see cref="ItemRequest"/>
    /// </summary>
    /// <param name="request"></param>
    public async Task<Item> CreateItem(ItemRequest request)
    {
        var user = await _authenticateService.GetCurrentUser();
        if (user == null) throw new NotAuthenticatedException("User not found.");

        // Fill request data
        Item item = new();
        await FillItemRequest(item, request);

        // Set initial status
        item.Status = ItemStatus.Public;

        // Set owner
        item.Owner = user;

        // Create the item  
        await _itemService.Create(item);

        return item;
    }

    public async Task UpdateItem(Item item, ItemRequest request)
    {
        await FillItemRequest(item, request);

        // Item updated so we need to approve it
        if (item.Status == ItemStatus.Denied)
            item.Status = ItemStatus.Approving;

        // Update the item
        await _itemService.Update(item);
    }

    public async Task DeleteItem(Item item)
    {
        if(!await CanDelete(item))
            throw new InvalidOperationException("Item cannot be deleted because there are running loans.");

        // Delete the item
        await _itemService.Delete(item);
    }
    
    public async Task<bool> CanDelete(Item item)
    {
        // Check if the item has any running loans
        var runningLoans = await _loanService.GetRunningLoansCountByItem(item);
        
        // Return true if there are no running loans
        return runningLoans == 0;
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
    
    public async Task AddImage(Item item, Image image)
    {
        // Check that the item has not reached the maximum number of images
        if (item.Images.Count >= _configuration.MaxImagesPerItem)
            throw new ArgumentException("Max images per item exceeded.");
        
        // Make item owner the owner of the image
        image.Owner = item.Owner;
        
        // Add image to the item
        item.Images.Add(image);

        // Update the item
        await _itemService.Update(item);
    }
}
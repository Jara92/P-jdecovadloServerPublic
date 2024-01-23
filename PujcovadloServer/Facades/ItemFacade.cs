using System.Data;
using AutoMapper;
using PujcovadloServer.Enums;
using PujcovadloServer.Exceptions;
using PujcovadloServer.Helpers;
using PujcovadloServer.Models;
using PujcovadloServer.Repositories.Interfaces;
using PujcovadloServer.Requests;
using PujcovadloServer.Services;

namespace PujcovadloServer.Facades;

public class ItemFacade
{
    private readonly IItemRepository _itemRepository;
    private readonly ItemService _itemService;
    private readonly IMapper _mapper;

    public ItemFacade(IItemRepository itemRepository, ItemService itemService, IMapper mapper)
    {
        _itemRepository = itemRepository;
        _itemService = itemService;
        _mapper = mapper;
    }

    /// <summary>
    /// Creates a new item using <see cref="ItemRequest"/>
    /// </summary>
    /// <param name="request"></param>
    public async Task<Item> CreateItem(ItemRequest request)
    {
        // Map request to item
        var item = _mapper.Map<Item>(request);

        // Set initial status
        item.Status = ItemStatus.Public;

        // Set alias
        item.Alias = UrlHelper.CreateUrlStub(item.Name);

        // Create the item  
        await _itemService.Create(item);

        return item;
    }

    public async Task UpdateItem(ItemRequest request)
    {
        // Updated tracked item
        var item = await _itemRepository.Get(request.Id);

        if (item == null) throw new EntityNotFoundException($"Item with id {request.Id} not found.");

        // Map request to item
        item.Name = request.Name;
        item.Alias = UrlHelper.CreateUrlStub(item.Name);
        item.Description = request.Description;

        // update prices
        item.PricePerDay = request.PricePerDay;
        item.PurchasePrice = request.PurchasePrice;
        item.SellingPrice = request.SellingPrice;

        // Update categories
        item.Categories.Clear();
        foreach (var category in request.Categories)
        {
            item.Categories.Add(_mapper.Map<ItemCategory>(category));
        }
        
        // Item updated so we need to approve it
        if(item.Status == ItemStatus.Denied)
            item.Status = ItemStatus.Approving;
        
        // Todo: update images
        
        // Update the item
        await _itemService.Update(item);
    }
    
    public async Task DeleteItem(int id)
    {
        // Get the item
        var item = await _itemService.Get(id);
        if (item == null) throw new EntityNotFoundException($"Item with id {id} not found.");
        
        // TODO: Check that the item can be deleted (no active rentals etc.)
        
        // Delete the item
        await _itemService.Delete(item);
    }

    public async Task AddCategory(Item item, ItemCategory dbCategory)
    {
        // Check if item already has this category.
        if (!item.Categories.Any(c => c.Id == dbCategory.Id))
        {
            item.Categories.Add(dbCategory);
            await _itemRepository.Update(item);
        }
    }
}
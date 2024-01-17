using AutoMapper;
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
    /// Create new Item.
    /// </summary>
    /// <param name="item"></param>
    public async Task CreateItem(Item item)
    {
        item.Alias = UrlHelper.CreateUrlStub(item.Name);

        await _itemRepository.Create(item);
    }
    
    /// <summary>
    /// Creates a new item using <see cref="ItemRequest"/>
    /// </summary>
    /// <param name="request"></param>
    public async Task CreateItem(ItemRequest request)
    {
        //var item = new Item();
        //_itemService.FillRequest(item, request);
        var item = _mapper.Map<Item>(request);
        
        await CreateItem(item);
    }

    public async Task UpdateItem(Item item, ItemRequest? request = null)
    {
        // Fill item with request data.
        if(request != null)
        {
             // Apply request data to item.
            _mapper.Map(request, item);
        }
        
        // Update item alias.
        item.Alias = UrlHelper.CreateUrlStub(item.Name);

        await _itemRepository.Update(item);
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
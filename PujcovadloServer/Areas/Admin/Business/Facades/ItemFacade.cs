using AutoMapper;
using NuGet.Packaging;
using PujcovadloServer.Areas.Admin.Requests;
using PujcovadloServer.Business.Entities;
using PujcovadloServer.Business.Exceptions;
using PujcovadloServer.Business.Filters;
using PujcovadloServer.Business.Interfaces;
using PujcovadloServer.Business.Objects;
using PujcovadloServer.Business.Services;
using PujcovadloServer.Helpers;

namespace PujcovadloServer.Areas.Admin.Business.Facades;

public class ItemFacade
{
    private readonly ItemService _itemService;
    private readonly ItemCategoryService _itemCategoryService;
    private readonly ItemTagService _itemTagService;
    private readonly ApplicationUserService _userService;
    private readonly IMapper _mapper;
    private readonly IFileStorage _storage;

    public ItemFacade(ItemService itemService, ItemCategoryService itemCategoryService,
        ItemTagService itemTagService, ApplicationUserService userService, IMapper mapper, IFileStorage storage)
    {
        _itemService = itemService;
        _itemCategoryService = itemCategoryService;
        _itemTagService = itemTagService;
        _userService = userService;
        _mapper = mapper;
        _storage = storage;
    }

    public async Task<Item> GetItem(int id)
    {
        var item = await _itemService.Get(id);
        if (item == null) throw new EntityNotFoundException();

        foreach (var image in item.Images)
        {
            image.Url = await _storage.GetFilePublicUrl("images", image.Path);
        }

        return item;
    }


    public async Task UpdateItem(Item item, ItemRequest request)
    {
        // map the request to the item
        _mapper.Map(request, item);

        // Generate the alias if it is not set
        if (string.IsNullOrEmpty(item.Alias))
        {
            item.Alias = UrlHelper.CreateUrlStub(item.Name);
        }

        // sync categories
        item.Categories.Clear();
        item.Categories.AddRange(await _itemCategoryService.GetByIds(request.Categories));

        // sync tags
        item.Tags.Clear();
        item.Tags.AddRange(await _itemTagService.GetByIds(request.Tags));

        await _itemService.Update(item);
    }

    public Task Delete(Item item)
    {
        return _itemService.Delete(item);
    }

    public Task<List<EntityOption>> GetItemCategoryOptions()
    {
        return _itemCategoryService.GetAllOptions();
    }

    public Task<IList<ItemTagOption>> GetItemTagOptions()
    {
        return _itemTagService.GetAllAsOptions();
    }

    public Task<IList<ApplicationUserOption>> GetUserOptions()
    {
        return _userService.GetAllAsOptions(new ApplicationUserFilter());
    }

    public Task<List<EntityOption>> GetItemStatusOptions()
    {
        return _itemService.GetItemStatusOptions();
    }
}
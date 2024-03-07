using AutoMapper;
using PujcovadloServer.Areas.Admin.Requests;
using PujcovadloServer.Business.Entities;
using PujcovadloServer.Business.Exceptions;
using PujcovadloServer.Business.Interfaces;
using PujcovadloServer.Business.Services;
using PujcovadloServer.Helpers;

namespace PujcovadloServer.Areas.Admin.Business.Facades;

public class ItemCategoryFacade
{
    private readonly ItemCategoryService _itemCategoryService;
    private readonly ItemTagService _itemTagService;
    private readonly ApplicationUserService _userService;
    private readonly IMapper _mapper;
    private readonly IFileStorage _storage;

    public ItemCategoryFacade(ItemCategoryService itemCategoryService,
        ItemTagService itemTagService, ApplicationUserService userService, IMapper mapper, IFileStorage storage)
    {
        _itemCategoryService = itemCategoryService;
        _itemTagService = itemTagService;
        _userService = userService;
        _mapper = mapper;
        _storage = storage;
    }

    public async Task<ItemCategory> Get(int id)
    {
        var category = await _itemCategoryService.Get(id);
        if (category == null) throw new EntityNotFoundException();

        return category;
    }

    public async Task<ItemCategory> Create(ItemCategoryRequest request)
    {
        var category = new ItemCategory();
        await FillRequest(category, request);

        await _itemCategoryService.Create(category);

        return category;
    }

    private async Task FillRequest(ItemCategory category, ItemCategoryRequest request)
    {
        // map the request to the category
        category.Name = request.Name;
        category.Alias = request.Alias;
        category.Description = request.Description;

        // if the parent category is set, check if it exists
        if (request.ParentId.HasValue)
        {
            // Cant set the parent category to itself
            if (request.ParentId.Value == category.Id)
            {
                throw new ArgumentException("Parent category cannot be the category itself");
            }

            // get the parent category
            var parentCategory = await _itemCategoryService.Get(request.ParentId.Value);

            // if the parent category does not exist, throw an exception
            if (parentCategory == null)
            {
                throw new ArgumentException("Parent category not found");
            }

            // set the parent category if it exists
            category.ParentId = request.ParentId.Value;
        }
        // Set the parent category to null if it is not set
        else
        {
            category.ParentId = null;
        }

        // Generate the alias if it is not set
        if (string.IsNullOrEmpty(category.Alias))
        {
            category.Alias = UrlHelper.CreateUrlStub(category.Name);
        }
    }

    public async Task Update(ItemCategory category, ItemCategoryRequest request)
    {
        await FillRequest(category, request);

        await _itemCategoryService.Update(category);
    }

    public Task Delete(ItemCategory category)
    {
        return _itemCategoryService.Delete(category);
    }
}
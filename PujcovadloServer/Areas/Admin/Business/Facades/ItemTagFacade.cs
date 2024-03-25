using AutoMapper;
using PujcovadloServer.Areas.Admin.Requests;
using PujcovadloServer.Business.Entities;
using PujcovadloServer.Business.Exceptions;
using PujcovadloServer.Business.Filters;
using PujcovadloServer.Business.Objects;
using PujcovadloServer.Business.Services;
using PujcovadloServer.Lib;

namespace PujcovadloServer.Areas.Admin.Business.Facades;

public class ItemTagFacade
{
    private readonly ItemTagService _itemTagService;
    private readonly IMapper _mapper;

    public ItemTagFacade(ItemTagService itemTagService, IMapper mapper)
    {
        _itemTagService = itemTagService;
        _mapper = mapper;
    }

    public Task<PaginatedList<ItemTag>> GetAll(ItemTagFilter filter)
    {
        // Display only approved items
        filter.OnlyApproved = true;

        return _itemTagService.GetAll(filter);
    }

    public async Task<ItemTag> Get(int id)
    {
        var tag = await _itemTagService.Get(id);
        if (tag == null) throw new EntityNotFoundException();

        return tag;
    }

    public async Task<ItemTag> Create(ItemTagRequest request)
    {
        var tag = new ItemTag();
        await FillRequest(tag, request);

        await _itemTagService.Create(tag);

        return tag;
    }

    private Task FillRequest(ItemTag tag, ItemTagRequest request)
    {
        // map the request
        tag.Name = request.Name;
        tag.IsApproved = request.IsApproved;

        return Task.CompletedTask;
    }

    public async Task Update(ItemTag category, ItemTagRequest request)
    {
        await FillRequest(category, request);

        await _itemTagService.Update(category);
    }

    public Task Delete(ItemTag category)
    {
        return _itemTagService.Delete(category);
    }

    public Task<IList<ItemTagOption>> GetItemTagOptions()
    {
        return _itemTagService.GetAllAsOptions();
    }
}
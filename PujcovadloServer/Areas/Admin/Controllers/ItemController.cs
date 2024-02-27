using AutoMapper;
using Core.Flash2;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using PujcovadloServer.Areas.Admin.Business.Filters;
using PujcovadloServer.Areas.Admin.Enums;
using PujcovadloServer.Areas.Admin.Facades;
using PujcovadloServer.Areas.Admin.Requests;
using PujcovadloServer.Areas.Admin.ViewModels;
using PujcovadloServer.Business.Entities;
using PujcovadloServer.Business.Enums;
using X.PagedList;

namespace PujcovadloServer.Areas.Admin.Controllers;

[Controller]
[Route("admin/items")]
[Area("Admin")]
[Authorize(Roles = UserRoles.Admin, AuthenticationSchemes = "Admin")]
public class ItemController : Controller
{
    private readonly ItemFacade _itemFacade;
    private readonly IMapper _mapper;
    private readonly IFlasher _flasher;
    private readonly IStringLocalizer<ItemController> _localizer;

    public ItemController(ItemFacade itemFacade, IMapper mapper, IFlasher flasher,
        IStringLocalizer<ItemController> localizer)
    {
        _itemFacade = itemFacade;
        _mapper = mapper;
        _flasher = flasher;
        _localizer = localizer;
    }

    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] ItemFilter filter)
    {
        var model = new ItemViewModel();

        // If model state is valid, get items by the filter
        if (ModelState.IsValid)
        {
            // Get items by filter
            var items = await _itemFacade.GetItems(filter);

            // Create paged list
            var usersAsIPagedList =
                new StaticPagedList<Item>(items, items.PageIndex, filter.PageSize, items.TotalCount);

            // Set view data
            model.Items = usersAsIPagedList;
        }
        // If model state is not valid, get use default filter isntead
        else
        {
            var tmpFilter = new ItemFilter();
            var items = await _itemFacade.GetItems(tmpFilter);

            // Create paged list
            var usersAsIPagedList =
                new StaticPagedList<Item>(items, items.PageIndex, tmpFilter.PageSize, items.TotalCount);

            // Set view data
            model.Items = usersAsIPagedList;
        }

        // Show real filter
        model.Filter = filter;

        return View(model);
    }

    private async Task PrepareViewData()
    {
        // get all users, categories and tags
        ViewData["Users"] = await _itemFacade.GetUserOptions();
        ViewData["Categories"] = await _itemFacade.GetItemCategoryOptions();
        ViewData["Tags"] = await _itemFacade.GetItemTagOptions();
    }

    [HttpGet("edit/{id}")]
    public async Task<IActionResult> Edit(int id)
    {
        // get the item
        var item = await _itemFacade.GetItem(id);

        // map the item to the request
        var model = _mapper.Map<Item, ItemRequest>(item);

        await PrepareViewData();

        return View(model);
    }

    [HttpPost("edit/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ItemRequest request)
    {
        if (ModelState.IsValid)
        {
            // get the item
            var item = await _itemFacade.GetItem(id);

            // update the item
            await _itemFacade.UpdateItem(item, request);

            _flasher.Flash(FlashType.Success, _localizer["Item has been updated."]);
        }
        else
        {
            _flasher.Flash(FlashType.Error, _localizer["Item cannot be updated because of errors."]);
        }

        await PrepareViewData();

        return View(request);
    }

    [HttpGet("delete/{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await _itemFacade.GetItem(id);

        return View(item);
    }

    [HttpPost("delete/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DoDelete(int id)
    {
        var item = await _itemFacade.GetItem(id);

        await _itemFacade.Delete(item);

        _flasher.Flash(FlashType.Success, _localizer["Item was deleted."]);

        return RedirectToAction(nameof(Index));
    }
}
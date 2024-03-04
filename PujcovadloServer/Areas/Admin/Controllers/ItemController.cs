using AutoMapper;
using Core.Flash2;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using PujcovadloServer.Areas.Admin.Business.Facades;
using PujcovadloServer.Areas.Admin.Enums;
using PujcovadloServer.Areas.Admin.Requests;
using PujcovadloServer.Areas.Admin.Responses;
using PujcovadloServer.Areas.Admin.Services;
using PujcovadloServer.Business.Entities;
using PujcovadloServer.Business.Enums;
using Syncfusion.EJ2.Base;

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
    private readonly ApplicationUserService _userService;
    private readonly ItemService _itemService;

    public ItemController(ItemFacade itemFacade, IMapper mapper, IFlasher flasher,
        IStringLocalizer<ItemController> localizer, ApplicationUserService userService, ItemService itemService)
    {
        _itemFacade = itemFacade;
        _mapper = mapper;
        _flasher = flasher;
        _localizer = localizer;
        _userService = userService;
        _itemService = itemService;
    }


    [HttpGet]
    public async Task<IActionResult> Index()
    {
        ViewBag.Statuses = await _itemService.GetItemStatusOptions();
        ViewBag.Users = await _userService.GetUserOptions();

        return View();
    }

    [HttpPost]
    public async Task<IActionResult> IndexFilter([FromBody] DataManagerRequest dm)
    {
        // get the items
        var items = await _itemService.GetItems(dm);

        // get total count of items which match the filter
        var itemsCOunt = await _itemService.GetItemsCount(dm);

        // get aggregations
        var aggregate = await _itemService.GetAggregations(dm);

        // map the items to the response
        var list = _mapper.Map<List<Item>, List<ItemResponse>>(items);

        return dm.RequiresCounts
            ? Json(new { result = list, count = itemsCOunt, aggregate })
            : Json(list);
    }

    [HttpPost("update")]
    public async Task<IActionResult> Update([FromBody] ViewModels.CRUDModel<ItemRequest> value)
    {
        if (!ModelState.IsValid)
        {
            var allErrors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage));
            return Json(allErrors);
        }

        var itemData = value.value;

        if (itemData.Id == null)
        {
            ModelState.AddModelError("Id", "Item ID is required.");
            var allErrors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage));
            return Json(allErrors);
        }

        // get the item
        var item = await _itemFacade.GetItem(itemData.Id.Value);

        // update the item
        await _itemFacade.UpdateItem(item, itemData);

        return Json(itemData);
    }

    [HttpPost("delete")]
    public async Task<IActionResult> Delete([FromBody] ViewModels.CRUDModel<ItemRequest> value)
    {
        if (!ModelState.IsValid)
        {
            var allErrors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage));
            return Json(allErrors);
        }

        if (value.key == null)
        {
            ModelState.AddModelError("Id", "Item ID is required.");
            var allErrors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage));
            return Json(allErrors);
        }

        // get the item
        var item = await _itemFacade.GetItem(int.Parse(value.key.ToString()));

        // update the item
        await _itemFacade.Delete(item);

        return Json(value);
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
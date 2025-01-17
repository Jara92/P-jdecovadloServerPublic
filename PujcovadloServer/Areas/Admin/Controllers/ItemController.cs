using AutoMapper;
using Core.Flash2;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using PujcovadloServer.Areas.Admin.Business.Facades;
using PujcovadloServer.Areas.Admin.Enums;
using PujcovadloServer.Areas.Admin.Requests;
using PujcovadloServer.Areas.Admin.Responses;
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
    private readonly PujcovadloServer.Business.Services.ItemService _itemService;

    public ItemController(ItemFacade itemFacade, IMapper mapper, IFlasher flasher,
        IStringLocalizer<ItemController> localizer, PujcovadloServer.Business.Services.ItemService itemService)
    {
        _itemFacade = itemFacade;
        _mapper = mapper;
        _flasher = flasher;
        _localizer = localizer;
        _itemService = itemService;
    }


    [HttpGet]
    public async Task<IActionResult> Index()
    {
        ViewBag.Statuses = await _itemFacade.GetItemStatusOptions();

        return View();
    }

    [HttpPost]
    public async Task<IActionResult> IndexFilter([FromBody] DataManagerRequest dm)
    {
        // get list of items
        var items = await _itemService.GetAll(dm);

        // map to response
        var result = _mapper.Map<List<Item>, List<ItemResponse>>(items);

        if (dm.RequiresCounts)
        {
            // get total count of items which match the filter
            var count = await _itemService.GetCount(dm);

            // get aggregations
            var aggregate = await _itemService.GetAggregations(dm);

            return Json(new { result, count, aggregate });
        }

        return Json(result);
    }

    private async Task PrepareViewData()
    {
        ViewBag.Statuses = await _itemFacade.GetItemStatusOptions();
    }

    [HttpGet("edit/{id}")]
    public async Task<IActionResult> Edit(int id)
    {
        // get the item
        var item = await _itemFacade.GetItem(id);

        // map the item to the request
        var model = _mapper.Map<Item, ItemRequest>(item);

        ViewBag.Images = item.Images;

        await PrepareViewData();

        return View(model);
    }

    [HttpPost("edit/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ItemRequest request)
    {
        // get the item
        var item = await _itemFacade.GetItem(id);
        if (ModelState.IsValid)
        {
            // update the item
            await _itemFacade.UpdateItem(item, request);

            _flasher.Flash(FlashType.Success, _localizer["Item has been updated."]);

            return RedirectToAction(nameof(Edit), new { id = item.Id });
        }

        // if there are errors, show the form again
        _flasher.Flash(FlashType.Error, _localizer["Item cannot be updated because of errors."]);

        await PrepareViewData();

        ViewBag.Images = item.Images;

        return View(request);
    }


    [HttpPost("delete/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await _itemFacade.GetItem(id);

        //return Problem("todo");

        await _itemFacade.Delete(item);

        _flasher.Flash(FlashType.Success, _localizer["Item was deleted."]);

        return RedirectToAction(nameof(Index));
    }
}
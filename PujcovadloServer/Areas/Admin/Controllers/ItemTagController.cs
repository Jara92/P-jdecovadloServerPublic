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
using PujcovadloServer.Business.Filters;
using Syncfusion.EJ2.Base;

namespace PujcovadloServer.Areas.Admin.Controllers;

[Controller]
[Route("admin/item-tags")]
[Area("Admin")]
[Authorize(Roles = UserRoles.Admin, AuthenticationSchemes = "Admin")]
public class ItemTagController : Controller
{
    private readonly ItemTagFacade _tagFacade;
    private readonly IMapper _mapper;
    private readonly IFlasher _flasher;
    private readonly IStringLocalizer<ItemTagController> _localizer;
    private readonly PujcovadloServer.Business.Services.ItemTagService _tagService;

    public ItemTagController(ItemTagFacade tagFacade, IMapper mapper, IFlasher flasher,
        IStringLocalizer<ItemTagController> localizer,
        PujcovadloServer.Business.Services.ItemTagService tagService)
    {
        _tagFacade = tagFacade;
        _mapper = mapper;
        _flasher = flasher;
        _localizer = localizer;
        _tagService = tagService;
    }


    [HttpGet]
    public async Task<IActionResult> Index()
    {
        await PrepareViewData();

        return View();
    }

    [HttpPost]
    public async Task<IActionResult> IndexFilter([FromBody] DataManagerRequest dm)
    {
        // get the items
        var categories = await _tagService.GetAll(dm);

        // get total count of items which match the filter
        var itemsCount = await _tagService.GetCount(dm);

        // get aggregations
        var aggregate = await _tagService.GetAggregations(dm);

        // map the items to the response
        var list = _mapper.Map<List<ItemTag>, List<ItemTagResponse>>(categories);

        return dm.RequiresCounts
            ? Json(new { result = list, count = itemsCount, aggregate })
            : Json(list);
    }

    private async Task PrepareViewData()
    {
        // get all users, categories and tags
        ViewBag.Categories = await _tagFacade.GetItemTagOptions();
    }

    [HttpGet("create")]
    public async Task<IActionResult> Create()
    {
        await PrepareViewData();

        return View("CreateOrEdit", new ItemTagRequest());
    }

    [HttpPost("create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ItemTagRequest request)
    {
        if (ModelState.IsValid)
        {
            var tag = await _tagFacade.Create(request);

            _flasher.Flash(FlashType.Success, _localizer["Tag has been created."]);

            return RedirectToAction(nameof(Edit), new { id = tag.Id });
        }

        _flasher.Flash(FlashType.Error, _localizer["Tag cannot be created because of errors."]);

        await PrepareViewData();

        return View("CreateOrEdit", request);
    }

    [HttpGet("edit/{id}")]
    public async Task<IActionResult> Edit(int id)
    {
        var tag = await _tagFacade.Get(id);

        // map the cat to the request
        var model = _mapper.Map<ItemTag, ItemTagRequest>(tag);

        await PrepareViewData();

        return View("CreateOrEdit", model);
    }

    [HttpPost("edit/{id}")]
    [ValidateAntiForgeryToken]
    [ValidateIdFilter]
    public async Task<IActionResult> Edit(int id, ItemTagRequest request)
    {
        var tag = await _tagFacade.Get(id);

        // Check if the model is valid
        if (!ModelState.IsValid)
        {
            _flasher.Flash(FlashType.Error, _localizer["Tag cannot be updated because of errors."]);

            await PrepareViewData();
            return View("CreateOrEdit", request);
        }

        // Update the tag
        await _tagFacade.Update(tag, request);

        // Display a success message
        _flasher.Flash(FlashType.Success, _localizer["Tag has been updated."]);

        // redirect to the edit page
        return RedirectToAction(nameof(Edit), new { id = tag.Id });
    }

    [HttpPost("delete/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var tag = await _tagFacade.Get(id);

        await _tagFacade.Delete(tag);

        _flasher.Flash(FlashType.Success, _localizer["Tag was deleted."]);

        return RedirectToAction(nameof(Index));
    }
}
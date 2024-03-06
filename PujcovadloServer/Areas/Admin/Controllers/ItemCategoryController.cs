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
[Route("admin/item-categories")]
[Area("Admin")]
[Authorize(Roles = UserRoles.Admin, AuthenticationSchemes = "Admin")]
public class ItemCategoryController : Controller
{
    private readonly ItemCategoryFacade _categoryFacade;
    private readonly IMapper _mapper;
    private readonly IFlasher _flasher;
    private readonly IStringLocalizer<ItemCategoryController> _localizer;
    private readonly PujcovadloServer.Business.Services.ItemCategoryService _itemCategoryService;

    public ItemCategoryController(ItemCategoryFacade categoryFacade, IMapper mapper, IFlasher flasher,
        IStringLocalizer<ItemCategoryController> localizer,
        PujcovadloServer.Business.Services.ItemCategoryService itemCategoryService)
    {
        _categoryFacade = categoryFacade;
        _mapper = mapper;
        _flasher = flasher;
        _localizer = localizer;
        _itemCategoryService = itemCategoryService;
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
        var categories = await _itemCategoryService.GetAll(dm);

        // get total count of items which match the filter
        var itemsCount = await _itemCategoryService.GetCount(dm);

        // get aggregations
        var aggregate = await _itemCategoryService.GetAggregations(dm);

        // map the items to the response
        var list = _mapper.Map<List<ItemCategory>, List<ItemCategoryResponse>>(categories);

        return dm.RequiresCounts
            ? Json(new { result = list, count = itemsCount, aggregate })
            : Json(list);
    }

    private async Task PrepareViewData()
    {
        // get all users, categories and tags
        ViewBag.Categories = await _categoryFacade.GetItemCategoryOptions();
    }

    [HttpGet("create")]
    public async Task<IActionResult> Create()
    {
        await PrepareViewData();

        return View("CreateOrEdit", new ItemCategoryRequest());
    }

    [HttpPost("create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ItemCategoryRequest request)
    {
        if (ModelState.IsValid)
        {
            var category = await _categoryFacade.Create(request);

            _flasher.Flash(FlashType.Success, _localizer["Category has been created."]);

            return RedirectToAction(nameof(Edit), new { id = category.Id });
        }

        _flasher.Flash(FlashType.Error, _localizer["Category cannot be created because of errors."]);

        await PrepareViewData();

        return View("CreateOrEdit", request);
    }

    [HttpGet("edit/{id}")]
    public async Task<IActionResult> Edit(int id)
    {
        var category = await _categoryFacade.Get(id);

        // map the cat to the request
        var model = _mapper.Map<ItemCategory, ItemCategoryRequest>(category);

        await PrepareViewData();

        return View("CreateOrEdit", model);
    }

    [HttpPost("edit/{id}")]
    [ValidateAntiForgeryToken]
    [ValidateIdFilter]
    public async Task<IActionResult> Edit(int id, ItemCategoryRequest request)
    {
        var category = await _categoryFacade.Get(id);

        // Check if the model is valid
        if (ModelState.IsValid)
        {
            // Update the category
            await _categoryFacade.Update(category, request);

            // Display a success message
            _flasher.Flash(FlashType.Success, _localizer["Category has been updated."]);

            // redirect to the edit page
            return RedirectToAction(nameof(Edit), new { id = category.Id });
        }

        // display an error message
        _flasher.Flash(FlashType.Error, _localizer["Category cannot be updated because of errors."]);

        await PrepareViewData();
        return View("CreateOrEdit", request);
    }

    [HttpPost("delete/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var category = await _categoryFacade.Get(id);

        await _categoryFacade.Delete(category);

        _flasher.Flash(FlashType.Success, _localizer["Category was deleted."]);

        return RedirectToAction(nameof(Index));
    }
}
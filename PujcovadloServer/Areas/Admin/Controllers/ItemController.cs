using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NuGet.Packaging;
using PujcovadloServer.Areas.Admin.Requests;
using PujcovadloServer.Business.Entities;
using PujcovadloServer.Business.Enums;
using PujcovadloServer.Business.Filters;
using PujcovadloServer.Business.Services;

namespace PujcovadloServer.Areas.Admin.Controllers;

[Controller]
[Route("admin/items")]
[Area("Admin")]
[Authorize(Roles = UserRoles.Admin, AuthenticationSchemes = "Admin")]
public class ItemController : Controller
{
    private readonly ItemService _itemService;
    private readonly ItemCategoryService _itemCategoryService;
    private readonly ItemTagService _itemTagService;
    private readonly ApplicationUserService _userService;
    private readonly IMapper _mapper;

    public ItemController(ItemService itemService, ItemCategoryService itemCategoryService,
        ItemTagService itemTagService,
        ApplicationUserService userService, IMapper mapper)
    {
        _itemService = itemService;
        _itemCategoryService = itemCategoryService;
        _itemTagService = itemTagService;
        _userService = userService;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        ViewData["Items"] = await _itemService.GetAll(new ItemFilter());

        return View();
    }

    [HttpGet("edit/{id}")]
    public async Task<IActionResult> Edit(int id)
    {
        var item = await _itemService.Get(id);
        if (item == null)
        {
            return NotFound();
        }

        var model = _mapper.Map<Item, ItemRequest>(item);

        ViewData["Users"] = await _userService.GetAllAsOptions(new ApplicationUserFilter());
        ViewData["Categories"] = await _itemCategoryService.GetAllOptions();
        ViewData["Tags"] = await _itemTagService.GetAllAsOptions();

        return View(model);
    }

    [HttpPost("edit/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ItemRequest request)
    {
        var item = await _itemService.Get(id);

        if (item == null || item.Id != id)
        {
            return NotFound();
        }

        //item.Owner = await _userService.Get(item.Owner.Id);
        //item.OwnerId = item.Owner.Id;

        if (ModelState.IsValid)
        {
            _mapper.Map(request, item);

            // sync categories
            item.Categories.Clear();
            item.Categories.AddRange(await _itemCategoryService.GetByIds(request.Categories));

            // sync tags
            item.Tags.Clear();
            item.Tags.AddRange(await _itemTagService.GetByIds(request.Tags));

            await _itemService.Update(item);

            // TODO: FLASH MESSAGE
        }
        else
        {
            // TODO: FLASH MESSAGE
        }

        ViewData["Users"] = await _userService.GetAllAsOptions(new ApplicationUserFilter());
        ViewData["Categories"] = await _itemCategoryService.GetAllOptions();
        ViewData["Tags"] = await _itemTagService.GetAllAsOptions();

        return View(request);
    }

    [HttpGet("delete/{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await _itemService.Get(id);
        if (item == null)
        {
            return NotFound();
        }

        return View(item);
    }

    [HttpPost("delete/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, Item item)
    {
        if (id != item.Id)
        {
            return NotFound();
        }

        // TODO: display flash message (https://github.com/lurumad/core-flash)

        await _itemService.Delete(item);
        return RedirectToAction(nameof(Index));
    }
}
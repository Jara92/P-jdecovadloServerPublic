using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PujcovadloServer.Areas.Admin.Facades;
using PujcovadloServer.Areas.Admin.Requests;
using PujcovadloServer.Business.Entities;
using PujcovadloServer.Business.Enums;
using PujcovadloServer.Business.Filters;

namespace PujcovadloServer.Areas.Admin.Controllers;

[Controller]
[Route("admin/items")]
[Area("Admin")]
[Authorize(Roles = UserRoles.Admin, AuthenticationSchemes = "Admin")]
public class ItemController : Controller
{
    private readonly ItemFacade _itemFacade;
    private readonly IMapper _mapper;

    public ItemController(ItemFacade itemFacade, IMapper mapper)
    {
        _itemFacade = itemFacade;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] ItemFilter filter)
    {
        ViewData["Filter"] = filter;
        ViewData["Items"] = await _itemFacade.GetItems(filter);

        return View();
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

            // TODO: FLASH MESSAGE
        }
        else
        {
            // TODO: FLASH MESSAGE
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

        // TODO: display flash message (https://github.com/lurumad/core-flash)

        await _itemFacade.Delete(item);

        return RedirectToAction(nameof(Index));
    }
}
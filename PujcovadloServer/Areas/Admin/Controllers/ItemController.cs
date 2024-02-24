using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

    public ItemController(ItemService itemService)
    {
        _itemService = itemService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        ViewData["Items"] = await _itemService.GetAll(new ItemFilter());

        return View();
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Details(int id)
    {
        var item = await _itemService.Get(id);
        if (item == null)
        {
            return NotFound();
        }

        return View(item);
    }

    [HttpGet("create")]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost("create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Item item)
    {
        if (ModelState.IsValid)
        {
            await _itemService.Create(item);
            return RedirectToAction(nameof(Index));
        }

        return View(item);
    }

    [HttpGet("edit/{id}")]
    public async Task<IActionResult> Edit(int id)
    {
        var item = await _itemService.Get(id);
        if (item == null)
        {
            return NotFound();
        }

        return View(item);
    }

    [HttpPost("edit/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Item item)
    {
        if (id != item.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            await _itemService.Update(item);
            return RedirectToAction(nameof(Index));
        }

        return View(item);
    }
}
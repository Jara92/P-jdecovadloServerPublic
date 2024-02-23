using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PujcovadloServer.Business.Filters;
using PujcovadloServer.Business.Services;

namespace PujcovadloServer.Areas.Admin.Controllers;

[Controller]
[Route("admin/items")]
[AllowAnonymous]
[Area("Admin")]
public class ItemAdminController : Controller
{
    private readonly ItemService _itemService;

    public ItemAdminController(ItemService itemService)
    {
        _itemService = itemService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Index()
    {
        ViewData["Items"] = await _itemService.GetAll(new ItemFilter());

        return View();
    }
}
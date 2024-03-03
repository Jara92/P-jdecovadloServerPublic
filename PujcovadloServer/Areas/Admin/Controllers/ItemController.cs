using System.Collections;
using AutoMapper;
using Core.Flash2;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using PujcovadloServer.Areas.Admin.Business.Facades;
using PujcovadloServer.Areas.Admin.Enums;
using PujcovadloServer.Areas.Admin.Requests;
using PujcovadloServer.Areas.Admin.Responses;
using PujcovadloServer.Business.Entities;
using PujcovadloServer.Business.Enums;
using PujcovadloServer.Data;
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
    private readonly IStringLocalizer<ItemStatus> _itemStatusLocalizer;
    private readonly IStringLocalizer<ItemController> _localizer;
    private readonly IConfiguration _configuration;
    private readonly PujcovadloServerContext _dbContext;

    public ItemController(ItemFacade itemFacade, IMapper mapper, IFlasher flasher,
        IStringLocalizer<ItemStatus> itemStatusLocalizer,
        IStringLocalizer<ItemController> localizer, IConfiguration configuration, PujcovadloServerContext dbContext)
    {
        _itemFacade = itemFacade;
        _mapper = mapper;
        _flasher = flasher;
        _itemStatusLocalizer = itemStatusLocalizer;
        _localizer = localizer;
        _configuration = configuration;
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var statuses = new List<object>();
        foreach (var i in Enum.GetValues(typeof(ItemStatus)))
        {
            statuses.Add(new
            {
                Text = _itemStatusLocalizer[i.ToString()].Value,
                Value = i
            });
        }

        ViewBag.Statuses = statuses;

        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> IndexFilter([FromBody] DataManagerRequest dm)
    {
        var data = _dbContext.Item.AsQueryable()
                .Select(i => new ItemResponse
                {
                    Id = i.Id,
                    Name = i.Name,
                    Alias = i.Alias,
                    Status = i.Status
                })
            ;

        var operations = new DataOperations();

        if (dm.Search != null && dm.Search.Count > 0)
        {
            data = operations.PerformSearching(data, dm.Search); //Search
        }

        if (dm.Sorted != null && dm.Sorted.Count > 0) //Sorting
        {
            data = operations.PerformSorting(data, dm.Sorted);
        }

        if (dm.Where != null && dm.Where.Count > 0) //Filtering
        {
            data = operations.PerformFiltering(data, dm.Where, dm.Where[0].Operator);
        }

        var itemsCOunt = await data.CountAsync();

        List<string> str = new List<string>();
        if (dm.Aggregates != null)
        {
            for (var i = 0; i < dm.Aggregates.Count; i++)
                str.Add(dm.Aggregates[i].Field);
        }

        IEnumerable aggregate = operations.PerformSelect(data, str);

        if (dm.Skip != 0)
        {
            data = operations.PerformSkip(data, dm.Skip); //Paging
        }

        if (dm.Take != 0)
        {
            data = operations.PerformTake(data, dm.Take);
        }

        var list = await data.ToListAsync();

        var responseList = list;
        /*var responseList = list.Select(i => new ItemResponse
        {
            Id = i.Id,
            Name = i.Name,
            Alias = i.Alias,
            Status = i.Status.ToString()
        }).ToList();*/
        //  var responseList = _mapper.Map<List<Item>, List<ItemResponse>>(list);

        return dm.RequiresCounts
            ? Json(Newtonsoft.Json.JsonConvert.SerializeObject(new
                { result = responseList, count = itemsCOunt, aggregate }))
            : Json(Newtonsoft.Json.JsonConvert.SerializeObject(responseList));
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
using AutoMapper;
using Core.Flash2;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using PujcovadloServer.Areas.Admin.Business.Facades;
using PujcovadloServer.Areas.Admin.Enums;
using PujcovadloServer.Areas.Admin.Requests;
using PujcovadloServer.Areas.Admin.Responses;
using PujcovadloServer.Authentication;
using PujcovadloServer.Business.Enums;
using Syncfusion.EJ2.Base;

namespace PujcovadloServer.Areas.Admin.Controllers;

[Controller]
[Route("admin/users")]
[Area("Admin")]
[Authorize(Roles = UserRoles.Admin, AuthenticationSchemes = "Admin")]
public class UserController : Controller
{
    private readonly UserFacade _userFacade;
    private readonly IMapper _mapper;
    private readonly IFlasher _flasher;
    private readonly IStringLocalizer<UserController> _localizer;
    private readonly PujcovadloServer.Business.Services.ApplicationUserService _userService;

    public UserController(UserFacade userFacade, IMapper mapper, IFlasher flasher,
        IStringLocalizer<UserController> localizer,
        PujcovadloServer.Business.Services.ApplicationUserService userService)
    {
        _userFacade = userFacade;
        _mapper = mapper;
        _flasher = flasher;
        _localizer = localizer;
        _userService = userService;
    }


    [HttpGet]
    public async Task<IActionResult> Index()
    {
        ViewBag.Roles = _userService.GetRoleOptions();

        return View();
    }

    [HttpPost]
    public async Task<IActionResult> IndexFilter([FromBody] DataManagerRequest dm)
    {
        // get the loans
        var users = await _userService.GetAll(dm);

        // get total count of loans which match the filter
        var count = await _userService.GetCount(dm);

        // get aggregations
        var aggregate = await _userService.GetAggregations(dm);

        // map the loans to the response
        var list = _mapper.Map<List<ApplicationUser>, List<UserResponse>>(users);

        return dm.RequiresCounts
            ? Json(new { result = list, count, aggregate })
            : Json(list);
    }

    private async Task PrepareViewData()
    {
        // get all roles
        ViewBag.Roles = UserRoles.AllRoles;
    }

    [HttpGet("create")]
    public async Task<IActionResult> Create()
    {
        await PrepareViewData();

        return View("CreateOrEdit", new UserRequest());
    }

    [HttpPost("create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(UserRequest request)
    {
        if (ModelState.IsValid)
        {
            var user = await _userFacade.Create(request);

            _flasher.Flash(FlashType.Success, _localizer["User has been created."]);

            return RedirectToAction(nameof(Edit), new { id = user.Id });
        }

        _flasher.Flash(FlashType.Error, _localizer["User cannot be created because of errors."]);

        await PrepareViewData();

        return View("CreateOrEdit", request);
    }

    [HttpGet("edit/{id}")]
    public async Task<IActionResult> Edit(string id)
    {
        var user = await _userFacade.Get(id);

        // map the cat to the request
        var model = _mapper.Map<ApplicationUser, UserRequest>(user);

        model.Roles = await _userFacade.GetUserRoles(user);

        await PrepareViewData();

        return View("CreateOrEdit", model);
    }

    [HttpPost("edit/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, UserRequest request)
    {
        var user = await _userFacade.Get(id);

        // Check if the model is valid
        if (ModelState.IsValid)
        {
            // Update the user
            await _userFacade.Update(user, request);

            // Display a success message
            _flasher.Flash(FlashType.Success, _localizer["User has been updated."]);

            // redirect to the edit page
            return RedirectToAction(nameof(Edit), new { id = user.Id });
        }

        // Display errors
        _flasher.Flash(FlashType.Error, _localizer["User cannot be updated because of errors."]);

        await PrepareViewData();
        return View("CreateOrEdit", request);
    }


    [HttpPost("delete/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id)
    {
        var user = await _userFacade.Get(id);

        await _userFacade.Delete(user);

        _flasher.Flash(FlashType.Success, _localizer["Loan was deleted."]);

        return RedirectToAction(nameof(Index));
    }
}
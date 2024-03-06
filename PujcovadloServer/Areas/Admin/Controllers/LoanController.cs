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
[Route("admin/loans")]
[Area("Admin")]
[Authorize(Roles = UserRoles.Admin, AuthenticationSchemes = "Admin")]
public class LoanController : Controller
{
    private readonly LoanFacade _loanFacade;
    private readonly IMapper _mapper;
    private readonly IFlasher _flasher;
    private readonly IStringLocalizer<LoanController> _localizer;
    private readonly PujcovadloServer.Business.Services.LoanService _loanService;

    public LoanController(LoanFacade loanFacade, IMapper mapper, IFlasher flasher,
        IStringLocalizer<LoanController> localizer, PujcovadloServer.Business.Services.LoanService loanService)
    {
        _loanFacade = loanFacade;
        _mapper = mapper;
        _flasher = flasher;
        _localizer = localizer;
        _loanService = loanService;
    }


    [HttpGet]
    public async Task<IActionResult> Index()
    {
        ViewBag.Statuses = await _loanFacade.GetLoanStatusOptions();
        ViewBag.Users = await _loanFacade.GetUserOptions();

        return View();
    }

    [HttpPost]
    public async Task<IActionResult> IndexFilter([FromBody] DataManagerRequest dm)
    {
        // get the loans
        var loans = await _loanService.GetAll(dm);

        // get total count of loans which match the filter
        var count = await _loanService.GetCount(dm);

        // get aggregations
        var aggregate = await _loanService.GetAggregations(dm);

        // map the loans to the response
        var list = _mapper.Map<List<Loan>, List<LoanResponse>>(loans);

        return dm.RequiresCounts
            ? Json(new { result = list, count, aggregate })
            : Json(list);
    }

    private async Task PrepareViewData()
    {
        // get all users, categories and tags
        ViewBag.Statuses = await _loanFacade.GetLoanStatusOptions();
        ViewBag.Users = await _loanFacade.GetUserOptions();
    }

    [HttpGet("edit/{id}")]
    public async Task<IActionResult> Edit(int id)
    {
        // get the loan
        var loan = await _loanFacade.Get(id);

        // map the loan to the request
        var model = _mapper.Map<Loan, LoanRequest>(loan);

        await PrepareViewData();

        return View(model);
    }

    [HttpPost("edit/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, LoanRequest request)
    {
        // get the loan
        var loan = await _loanFacade.Get(id);
        if (ModelState.IsValid)
        {
            // update the loan
            await _loanFacade.Update(loan, request);

            _flasher.Flash(FlashType.Success, _localizer["Loan has been updated."]);

            return RedirectToAction(nameof(Edit), new { id });
        }

        // Display errors
        _flasher.Flash(FlashType.Error, _localizer["Loan cannot be updated because of errors."]);

        await PrepareViewData();

        return View(request);
    }


    [HttpPost("delete/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var loan = await _loanFacade.Get(id);

        await _loanFacade.Delete(loan);

        _flasher.Flash(FlashType.Success, _localizer["Loan was deleted."]);

        return RedirectToAction(nameof(Index));
    }
}
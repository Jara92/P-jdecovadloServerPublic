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
[Route("admin/reviews")]
[Area("Admin")]
[Authorize(Roles = UserRoles.Admin, AuthenticationSchemes = "Admin")]
public class ReviewController : Controller
{
    private readonly ReviewFacade _reviewFacade;
    private readonly IMapper _mapper;
    private readonly IFlasher _flasher;
    private readonly IStringLocalizer<ReviewController> _localizer;
    private readonly PujcovadloServer.Business.Services.ReviewService _reviewService;

    public ReviewController(ReviewFacade reviewFacade, IMapper mapper, IFlasher flasher,
        IStringLocalizer<ReviewController> localizer, PujcovadloServer.Business.Services.ReviewService reviewService)
    {
        _reviewFacade = reviewFacade;
        _mapper = mapper;
        _flasher = flasher;
        _localizer = localizer;
        _reviewService = reviewService;
    }


    [HttpGet]
    public async Task<IActionResult> Index()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> IndexFilter([FromBody] DataManagerRequest dm)
    {
        // get list of items
        var reviews = await _reviewService.GetAll(dm);

        // map to response
        var result = _mapper.Map<List<Review>, List<ReviewResponse>>(reviews);

        if (dm.RequiresCounts)
        {
            // get total count of items which match the filter
            var count = await _reviewService.GetCount(dm);

            // get aggregations
            var aggregate = await _reviewService.GetAggregations(dm);

            return Json(new { result, count, aggregate });
        }

        return Json(result);
    }

    [HttpGet("edit/{id}")]
    public async Task<IActionResult> Edit(int id)
    {
        // get the item
        var review = await _reviewFacade.Get(id);

        // map the item to the request
        var model = _mapper.Map<Review, ReviewRequest>(review);

        return View(model);
    }

    [HttpPost("edit/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ReviewRequest request)
    {
        // get the review
        var review = await _reviewFacade.Get(id);
        if (ModelState.IsValid)
        {
            // update the review
            await _reviewFacade.Update(review, request);

            _flasher.Flash(FlashType.Success, _localizer["Review has been updated."]);

            return RedirectToAction(nameof(Edit), new { id = review.Id });
        }

        // if there are errors, show the form again
        _flasher.Flash(FlashType.Error, _localizer["Item cannot be updated because of errors."]);

        return View(request);
    }


    [HttpPost("delete/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var review = await _reviewFacade.Get(id);

        await _reviewFacade.Delete(review);

        _flasher.Flash(FlashType.Success, _localizer["Review was deleted."]);

        return RedirectToAction(nameof(Index));
    }
}
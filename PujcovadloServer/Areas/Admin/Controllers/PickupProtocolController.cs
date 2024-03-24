using AutoMapper;
using Core.Flash2;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using PujcovadloServer.Areas.Admin.Business.Facades;
using PujcovadloServer.Areas.Admin.Enums;
using PujcovadloServer.Areas.Admin.Requests;
using PujcovadloServer.Business.Entities;
using PujcovadloServer.Business.Enums;
using PujcovadloServer.Filters;

namespace PujcovadloServer.Areas.Admin.Controllers;

[Controller]
[Route("admin/pickup-protocols")]
[Area("Admin")]
[Authorize(Roles = UserRoles.Admin, AuthenticationSchemes = "Admin")]
public class PickupProtocolController : Controller
{
    private readonly PickupProtocolFacade _protocolFacade;
    private readonly IMapper _mapper;
    private readonly IFlasher _flasher;
    private readonly IStringLocalizer<PickupProtocolController> _localizer;
    private readonly PujcovadloServer.Business.Services.PickupProtocolService _protocolService;

    public PickupProtocolController(PickupProtocolFacade protocolFacade, IMapper mapper, IFlasher flasher,
        IStringLocalizer<PickupProtocolController> localizer,
        PujcovadloServer.Business.Services.PickupProtocolService protocolService)
    {
        _protocolFacade = protocolFacade;
        _mapper = mapper;
        _flasher = flasher;
        _localizer = localizer;
        _protocolService = protocolService;
    }

    [NonAction]
    private Task PrepareView(PickupProtocol protocol)
    {
        ViewBag.Images = protocol.Images;

        return Task.CompletedTask;
    }

    [HttpGet("edit/{id}")]
    public async Task<IActionResult> Edit(int id)
    {
        var protocol = await _protocolFacade.Get(id);

        // map the cat to the request
        var model = _mapper.Map<PickupProtocol, PickupProtocolRequest>(protocol);

        await PrepareView(protocol);

        return View("CreateOrEdit", model);
    }

    [HttpPost("edit/{id}")]
    [ValidateAntiForgeryToken]
    [ValidateIdFilter]
    public async Task<IActionResult> Edit(int id, PickupProtocolRequest request)
    {
        var protocol = await _protocolFacade.Get(id);

        // Check if the model is valid
        if (ModelState.IsValid)
        {
            // Update the protocol
            await _protocolFacade.Update(protocol, request);

            // Display a success message
            _flasher.Flash(FlashType.Success, _localizer["Pickup protocol has been updated."]);

            // redirect to the edit page
            return RedirectToAction(nameof(Edit), new { id = protocol.Id });
        }

        // Display errors
        _flasher.Flash(FlashType.Error, _localizer["Pickup protocol cannot be updated because of errors."]);

        await PrepareView(protocol);

        return View("CreateOrEdit", request);
    }
}
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
using PujcovadloServer.Business.Filters;

namespace PujcovadloServer.Areas.Admin.Controllers;

[Controller]
[Route("admin/return-protocols")]
[Area("Admin")]
[Authorize(Roles = UserRoles.Admin, AuthenticationSchemes = "Admin")]
public class ReturnProtocolController : Controller
{
    private readonly ReturnProtocolFacade _protocolFacade;
    private readonly IMapper _mapper;
    private readonly IFlasher _flasher;
    private readonly IStringLocalizer<ReturnProtocolController> _localizer;
    private readonly PujcovadloServer.Business.Services.ReturnProtocolService _protocolService;

    public ReturnProtocolController(ReturnProtocolFacade protocolFacade, IMapper mapper, IFlasher flasher,
        IStringLocalizer<ReturnProtocolController> localizer,
        PujcovadloServer.Business.Services.ReturnProtocolService protocolService)
    {
        _protocolFacade = protocolFacade;
        _mapper = mapper;
        _flasher = flasher;
        _localizer = localizer;
        _protocolService = protocolService;
    }

    [NonAction]
    private Task PrepareView(ReturnProtocol protocol)
    {
        ViewBag.Images = protocol.Images;

        return Task.CompletedTask;
    }

    [HttpGet("edit/{id}")]
    public async Task<IActionResult> Edit(int id)
    {
        var protocol = await _protocolFacade.Get(id);

        // map the cat to the request
        var model = _mapper.Map<ReturnProtocol, ReturnProtocolRequest>(protocol);

        await PrepareView(protocol);

        return View("CreateOrEdit", model);
    }

    [HttpPost("edit/{id}")]
    [ValidateAntiForgeryToken]
    [ValidateIdFilter]
    public async Task<IActionResult> Edit(int id, ReturnProtocolRequest request)
    {
        var protocol = await _protocolFacade.Get(id);

        // Check if the model is valid
        if (!ModelState.IsValid)
        {
            _flasher.Flash(FlashType.Error, _localizer["Return protocol cannot be updated because of errors."]);

            await PrepareView(protocol);

            return View("CreateOrEdit", request);
        }

        // Update the protocol
        await _protocolFacade.Update(protocol, request);

        // Display a success message
        _flasher.Flash(FlashType.Success, _localizer["Return protocol has been updated."]);

        // redirect to the edit page
        return RedirectToAction(nameof(Edit), new { id = protocol.Id });
    }
}
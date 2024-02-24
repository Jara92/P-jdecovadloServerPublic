using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PujcovadloServer.Business.Enums;
using PujcovadloServer.Business.Services.Interfaces;

namespace PujcovadloServer.Areas.Admin.Controllers;

[Controller]
[Route("admin")]
[Area("Admin")]
[Authorize(Roles = UserRoles.Admin, AuthenticationSchemes = "Admin")]
public class HomeController : Controller
{
    private readonly IAuthenticateService _authenticateService;

    public HomeController(IAuthenticateService authenticateService)
    {
        _authenticateService = authenticateService;
    }

    [HttpGet("dashboard")]
    public IActionResult Dashboard()
    {
        return View();
    }

    [HttpGet("")]
    [AllowAnonymous]
    public IActionResult Index()
    {
        // Redirect to dashboard if the user is authenticated
        if (User.Identity?.IsAuthenticated ?? false)
        {
            return RedirectToAction(nameof(Dashboard));
        }

        // Redirect to login page if the user is not authenticated
        return RedirectToAction(nameof(AuthenticateController.Login), "Authenticate");
    }
}
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PujcovadloServer.Authentication;
using PujcovadloServer.Business.Enums;

namespace PujcovadloServer.Areas.Admin.Controllers;

[Controller]
[Route("admin/")]
[Area("Admin")]
[AllowAnonymous]
public class AuthenticateController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly LinkGenerator _urlHelper;

    public AuthenticateController(UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager, LinkGenerator urlHelper)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _urlHelper = urlHelper;
    }

    [HttpGet("login")]
    public async Task<IActionResult> Login()
    {
        if (_signInManager.IsSignedIn(User))
        {
            // todo: 
        }

        return View();
    }

    [HttpPost("login")]
    /*[ValidateAntiForgeryToken]*/
    public async Task<IActionResult> PerformLogin(LoginRequest request)
    {
        if (_signInManager.IsSignedIn(User))
        {
            // todo: 
        }

        // Require the user to have a confirmed email before they can log on.
        var user = await _userManager.FindByNameAsync(request.Username);
        if (user != null)
        {
            // Check that the user is admin
            if (!await _userManager.IsInRoleAsync(user, UserRoles.Admin))
            {
                TempData["Errors"] = new List<String>() { "You are not authorized to access this page." };
                return RedirectToAction(nameof(Login), "Authenticate");
            }
        }

        // This doesn't count login failures towards account lockout
        // To enable password failures to trigger account lockout, change to shouldLockout: true
        var result = await _signInManager.PasswordSignInAsync(request.Username, request.Password, false, false);

        if (result.Succeeded)
        {
            return RedirectToAction("Index", "Item");
        }

        if (result.IsLockedOut)
        {
            // todo
        }

        if (result.RequiresTwoFactor)
        {
            // todo
        }

        ModelState.AddModelError("", "Invalid login attempt.");
        return View("Login");
    }

    [HttpGet("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();

        return RedirectToAction(nameof(Login));
    }
}
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
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
    private readonly IStringLocalizer<AuthenticateController> _localizer;

    public AuthenticateController(UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager, LinkGenerator urlHelper,
        IStringLocalizer<AuthenticateController> localizer)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _urlHelper = urlHelper;
        _localizer = localizer;
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
        // back to login page if model is not valid
        if (!ModelState.IsValid)
        {
            return View("Login");
        }

        /*if (ModelState.IsValid)
        {*/
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

        if (result.Succeeded && user != null)
        {
            // Get user roles
            var userRoles = await _userManager.GetRolesAsync(user);

            // Create claims
            var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.PrimarySid, user.Id),
                new Claim(ClaimTypes.Name, user.UserName),
            };

            // Add user roles to claims
            foreach (var userRole in userRoles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, userRole));
            }

            var claimsIdentity = new ClaimsIdentity(authClaims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                AllowRefresh = true,
                //       // Refreshing the authentication session should be allowed.

                //       //ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(10),
                //       // The time at which the authentication ticket expires. A 
                //       // value set here overrides the ExpireTimeSpan option of 
                //       // CookieAuthenticationOptions set with AddCookie.

                //       //IsPersistent = true,
                //       // Whether the authentication session is persisted across 
                //       // multiple requests. When used with cookies, controls
                //       // whether the cookie's lifetime is absolute (matching the
                //       // lifetime of the authentication ticket) or session-based.

                //       //IssuedUtc = <DateTimeOffset>,
                //       // The time at which the authentication ticket was issued.

                //       //RedirectUri = <string>
                //       // The full path or absolute URI to be used as an http 
                //       // redirect response value.
            };

            await HttpContext.SignInAsync(
                "Admin",
                new ClaimsPrincipal(claimsIdentity),
                authProperties
            );

            return RedirectToAction(nameof(HomeController.Dashboard), "Home");
        }

        if (result.IsLockedOut)
        {
            // todo
        }

        if (result.RequiresTwoFactor)
        {
            // todo
        }

        // Add error message
        ModelState.AddModelError("", _localizer["Invalid username or password."]);

        // Back to login page.
        return View("Login");
    }

    [HttpGet("logout")]
    public async Task<IActionResult> Logout()
    {
        // TODO: WTF
        // await _signInManager.SignOutAsync();
        await HttpContext.SignOutAsync("Admin");

        return RedirectToAction(nameof(Login));
    }
}
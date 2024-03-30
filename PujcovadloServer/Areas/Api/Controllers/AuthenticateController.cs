using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PujcovadloServer.Areas.Api.Filters;
using PujcovadloServer.Authentication;
using PujcovadloServer.Authentication.Exceptions;
using PujcovadloServer.Business.Services.Interfaces;
using PujcovadloServer.Responses;

namespace PujcovadloServer.Areas.Api.Controllers;

[Area("Api")]
[Route("api")]
[ApiController]
[AllowAnonymous]
[ServiceFilter(typeof(ExceptionFilter))]
public class AuthenticateController : ControllerBase
{
    private readonly IAuthenticateService _authenticateService;

    public AuthenticateController(IAuthenticateService authenticateService)
    {
        _authenticateService = authenticateService;
    }

    /// <summary>
    /// Performs login and returns JWT token.
    /// </summary>
    /// <param name="request">Login request</param>
    /// <returns>JWT token for authorization.</returns>
    /// <response code="200">Returns JWT token for authorization.</response>
    /// <response code="401">Invalid credentials.</response>
    [HttpPost]
    [Route("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            // Try to login and get JWT token
            var result = await _authenticateService.Login(request);

            // Return token
            // TODO: make response for this
            return Ok(new
            {
                AccessToken = new JwtSecurityTokenHandler().WriteToken(result.Token),
                AccessTokenExpiration = result.Token.ValidTo,
                UserId = result.CurrentUser.Id
            });
        }
        catch (AuthenticationFailedException e)
        {
            var details = new ExceptionResponse
            {
                Title = "Unauthorized",
                Status = StatusCodes.Status401Unauthorized,
                Errors = new List<string> { e.Message }
            };

            return Unauthorized(details);
        }
    }

    /// <summary>
    /// Performs registration of a new user.
    /// </summary>
    /// <param name="request">Registration request.</param>
    /// <returns></returns>
    /// <response code="200">User was registered successfully.</response>
    /// <response code="400">Invalid request.</response>
    /// <response code="409">User with given credentials already exists.</response>
    [HttpPost]
    [Route("register")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        try
        {
            // Try register the user and get login request
            var result = await _authenticateService.RegisterUser(request);

            // Return token
            // TODO: make response for this
            return Ok(new
            {
                AccessToken = new JwtSecurityTokenHandler().WriteToken(result.Token),
                AccessTokenExpiration = result.Token.ValidTo,
                UserId = result.CurrentUser.Id
            });
        }
        catch (UsernameAlreadyExistsException e)
        {
            //ModelState.AddModelError(request.Username, "Username already exists");
            //return Conflict(ModelState);
            /*var details = new ExceptionResponse
            {
                Title = "Conflict",
                Status = StatusCodes.Status409Conflict,
                Errors = new List<Dictionary<>> { e.Message }
            };*/

            // TODO
            return Conflict(new
            {
                Errors = new Dictionary<String, List<String>>()
                {
                    {
                        nameof(request.Username), new List<String> { "Username is already taken." }
                    }
                }
            });
        }
        catch (EmailAlreadyExistsException)
        {
            return Conflict(new
            {
                Errors = new Dictionary<String, List<String>>()
                {
                    {
                        nameof(request.Email), new List<String> { "Email is already taken." }
                    }
                }
            });
        }
        catch (RegistrationFailedException e)
        {
            return BadRequest(e.Messages);
        }
    }
}
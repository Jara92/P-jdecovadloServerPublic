using Microsoft.AspNetCore.Mvc;
using PujcovadloServer.Areas.Api.Filters;
using PujcovadloServer.Areas.Api.Services;
using PujcovadloServer.AuthorizationHandlers;
using PujcovadloServer.AuthorizationHandlers.Profile;
using PujcovadloServer.Business.Facades;
using PujcovadloServer.Filters;
using PujcovadloServer.Requests;
using PujcovadloServer.Responses;

namespace PujcovadloServer.Areas.Api.Controllers;

/// <summary>
///  TODO: this sucks: make UserController and accept userid instead. It is ok if user has no profile.
/// </summary>
[Area("Api")]
[ApiController]
[Route("api/users")]
[ServiceFilter(typeof(ExceptionFilter))]
public class UserController : ControllerBase
{
    private readonly ProfileFacade _profileFacade;
    private readonly ProfileResponseGenerator _responseGenerator;
    private readonly AuthorizationService _authorizationService;

    public UserController(ProfileFacade profileFacade, ProfileResponseGenerator responseGenerator,
        AuthorizationService authorizationService)
    {
        _profileFacade = profileFacade;
        _responseGenerator = responseGenerator;
        _authorizationService = authorizationService;
    }

    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserResponse>> GetUser(string id)
    {
        // Get profile and check permissions
        var user = await _profileFacade.GetUserProfile(id);

        // Use is not visible if has no profile
        if (user.Profile == null) return NotFound();

        // Check that profile can be returned
        await _authorizationService.CheckPermissions(user.Profile, ProfileOperations.Read);

        // Generate response
        var response = await _responseGenerator.GenerateProfileDetailResponse(user);

        return Ok(response);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ValidateIdFilter]
    public async Task<ActionResult> UpdateProfile(string id, [FromBody] ProfileUpdateRequest request)
    {
        var user = await _profileFacade.GetUserProfile(id);

        // Cannot update user without a profile
        if (user.Profile == null) return BadRequest();

        // Check that user can update the profile
        await _authorizationService.CheckPermissions(user.Profile, ProfileOperations.Update);

        await _profileFacade.UpdateUserProfile(user, request);

        return NoContent();
    }
}
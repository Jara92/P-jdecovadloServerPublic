using Microsoft.AspNetCore.Mvc;
using PujcovadloServer.Areas.Api.Filters;
using PujcovadloServer.Areas.Api.Services;
using PujcovadloServer.AuthorizationHandlers;
using PujcovadloServer.AuthorizationHandlers.Profile;
using PujcovadloServer.Business.Facades;
using PujcovadloServer.Business.Filters;
using PujcovadloServer.Requests;
using PujcovadloServer.Responses;

namespace PujcovadloServer.Areas.Api.Controllers;

[Area("Api")]
[ApiController]
[Route("api/profiles")]
[ServiceFilter(typeof(ExceptionFilter))]
public class ProfileController : ControllerBase
{
    private readonly ProfileFacade _profileFacade;
    private readonly ProfileResponseGenerator _responseGenerator;
    private readonly AuthorizationService _authorizationService;

    public ProfileController(ProfileFacade profileFacade, ProfileResponseGenerator responseGenerator,
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
    public async Task<ActionResult<UserResponse>> GetProfile(int id)
    {
        // Get profile and check permissions
        var profile = await _profileFacade.GetProfile(id);
        await _authorizationService.CheckPermissions(profile, ProfileOperations.Read);

        // Generate response
        var response = await _responseGenerator.GenerateProfileDetailResponse(profile);

        return Ok(response);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ValidateIdFilter]
    public async Task<ActionResult> UpdateProfile(int id, [FromBody] ProfileUpdateRequest request)
    {
        var profile = await _profileFacade.GetProfile(id);

        await _authorizationService.CheckPermissions(profile, ProfileOperations.Update);

        await _profileFacade.UpdateProfile(profile, request);

        return NoContent();
    }
}
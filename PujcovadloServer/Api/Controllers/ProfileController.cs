using Microsoft.AspNetCore.Mvc;
using PujcovadloServer.Api.Filters;
using PujcovadloServer.Api.Services;
using PujcovadloServer.AuthorizationHandlers;
using PujcovadloServer.AuthorizationHandlers.Profile;
using PujcovadloServer.Business.Facades;
using PujcovadloServer.Business.Filters;
using PujcovadloServer.Requests;
using PujcovadloServer.Responses;

namespace PujcovadloServer.Api.Controllers;

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
    public async Task<ActionResult<LoanResponse>> GetProfile(int id)
    {
        // Get profile and check permissions
        var profile = await _profileFacade.GetProfile(id);
        await _authorizationService.CheckPermissions(profile, ProfileOperations.Read);

        // Get more detailed information about the profile
        var profileAggregations = await _profileFacade.GetProfileAggregations(profile);

        // Generate response
        var response = await _responseGenerator.GenerateProfileDetailResponse(profile, profileAggregations);

        return Ok(response);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ValidateIdFilter]
    public async Task<ActionResult> UpdateLoan(int id, [FromBody] ProfileUpdateRequest request)
    {
        var profile = await _profileFacade.GetProfile(id);

        await _authorizationService.CheckPermissions(profile, ProfileOperations.Update);

        await _profileFacade.UpdateProfile(profile, request);

        return NoContent();
    }
}
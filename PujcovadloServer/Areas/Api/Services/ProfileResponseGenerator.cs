using AutoMapper;
using PujcovadloServer.Areas.Api.Controllers;
using PujcovadloServer.AuthorizationHandlers;
using PujcovadloServer.Business.Facades;
using PujcovadloServer.Responses;
using Profile = PujcovadloServer.Business.Entities.Profile;

namespace PujcovadloServer.Areas.Api.Services;

public class ProfileResponseGenerator : ABaseResponseGenerator
{
    private readonly ProfileFacade _profileFacade;
    private readonly IMapper _mapper;

    public ProfileResponseGenerator(ProfileFacade profileFacade, IMapper mapper, LinkGenerator urlHelper,
        IHttpContextAccessor httpContextAccessor, AuthorizationService authorizationService) :
        base(httpContextAccessor, urlHelper, authorizationService)
    {
        _profileFacade = profileFacade;
        _mapper = mapper;
    }

    public async Task<UserResponse> GenerateProfileDetailResponse(Profile profile)
    {
        var response = _mapper.Map<UserResponse>(profile.User);

        if (response.Profile != null)
        {
            // Get more detailed information about the profile
            var aggregations = await _profileFacade.GetProfileAggregations(profile);
            response.Profile._aggregations = _mapper.Map<ProfileAggregationsResponse>(aggregations);
        }

        // User items link
        response._links.Add(new LinkResponse(
            _urlHelper.GetUriByAction(_httpContext, nameof(ItemController.Index), "Item",
                values: new { ownerId = profile.UserId }), "ITEMS", "GET"));

        // todo: more links

        return response;
    }
}
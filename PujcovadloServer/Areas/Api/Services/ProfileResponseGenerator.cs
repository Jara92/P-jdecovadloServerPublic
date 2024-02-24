using AutoMapper;
using PujcovadloServer.Areas.Api.Controllers;
using PujcovadloServer.AuthorizationHandlers;
using PujcovadloServer.Business.EntityAggregations;
using PujcovadloServer.Responses;
using Profile = PujcovadloServer.Business.Entities.Profile;

namespace PujcovadloServer.Areas.Api.Services;

public class ProfileResponseGenerator : ABaseResponseGenerator
{
    private readonly IMapper _mapper;

    public ProfileResponseGenerator(IMapper mapper, LinkGenerator urlHelper,
        IHttpContextAccessor httpContextAccessor, AuthorizationService authorizationService) :
        base(httpContextAccessor, urlHelper, authorizationService)
    {
        _mapper = mapper;
    }

    public async Task<ProfileResponse> GenerateProfileDetailResponse(Profile profile, ProfileAggregations? aggregations)
    {
        var response = _mapper.Map<ProfileResponse>(profile);
        response._aggregations = _mapper.Map<ProfileAggregationsResponse>(aggregations);

        // User items link
        response._links.Add(new LinkResponse(
            _urlHelper.GetUriByAction(_httpContext, nameof(ItemController.Index), "Item",
                values: new { ownerId = profile.UserId }), "ITEMS", "GET"));

        // todo: more links

        return response;
    }
}
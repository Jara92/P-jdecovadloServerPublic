using AutoMapper;
using PujcovadloServer.Business.EntityAggregations;
using PujcovadloServer.Business.Exceptions;
using PujcovadloServer.Business.Services;
using PujcovadloServer.Business.Services.Interfaces;
using PujcovadloServer.Requests;
using Profile = PujcovadloServer.Business.Entities.Profile;

namespace PujcovadloServer.Business.Facades;

public class ProfileFacade
{
    private readonly ProfileService _profileService;
    private readonly ItemService _itemService;
    private readonly ReviewService _reviewService;
    private readonly IAuthenticateService _authenticateService;
    private readonly IMapper _mapper;
    private readonly PujcovadloServerConfiguration _configuration;

    public ProfileFacade(ProfileService profileService, ItemService itemService, ReviewService reviewService,
        IAuthenticateService authenticateService, IMapper mapper, PujcovadloServerConfiguration configuration)
    {
        _profileService = profileService;
        _itemService = itemService;
        _reviewService = reviewService;
        _authenticateService = authenticateService;
        _mapper = mapper;
        _configuration = configuration;
    }

    public async Task<Profile> GetProfile(int profileId)
    {
        // Get image and check that it is not null
        var profile = await _profileService.Get(profileId);
        if (profile == null) throw new EntityNotFoundException("Image not found");

        return profile;
    }

    public async Task UpdateProfile(Profile profile, ProfileUpdateRequest request)
    {
        // Update profile
        profile.Description = request.Description;

        await _profileService.Update(profile);
    }

    public async Task<ProfileAggregations> GetProfileAggregations(Profile profile)
    {
        //TODO: Cache this information
        var publicItemsCount = await _itemService.GetPublicItemsCountByUser(profile.UserId);
        var averateRating = await _reviewService.GetAverageRatingForUser(profile.UserId);

        return new ProfileAggregations
        {
            CountOfPublicItems = publicItemsCount,
            AverageRating = averateRating
        };
    }
}
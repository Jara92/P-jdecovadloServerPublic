using AutoMapper;
using PujcovadloServer.Authentication;
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
    private readonly ApplicationUserService _userService;
    private readonly ItemService _itemService;
    private readonly ReviewService _reviewService;
    private readonly LoanService _loanService;
    private readonly IAuthenticateService _authenticateService;
    private readonly IMapper _mapper;
    private readonly PujcovadloServerConfiguration _configuration;

    public ProfileFacade(ProfileService profileService, ApplicationUserService userService, ItemService itemService,
        ReviewService reviewService,
        LoanService loanService, IAuthenticateService authenticateService, IMapper mapper,
        PujcovadloServerConfiguration configuration)
    {
        _profileService = profileService;
        _userService = userService;
        _itemService = itemService;
        _reviewService = reviewService;
        _loanService = loanService;
        _authenticateService = authenticateService;
        _mapper = mapper;
        _configuration = configuration;
    }

    public async Task<ApplicationUser> GetUserProfile(string userId)
    {
        var user = await _userService.Get(userId);
        if (user == null) throw new EntityNotFoundException("User not found");

        return user;
    }

    public async Task UpdateUserProfile(ApplicationUser user, ProfileUpdateRequest request)
    {
        var profile = user.Profile;

        // Just make sure profile is not null
        if (profile == null) throw new OperationNotAllowedException("Cannot update user without a profile");

        // Update profile
        profile.Description = request.Description;

        await _profileService.Update(profile);
    }

    public async Task<ProfileAggregations> GetProfileAggregations(Profile profile)
    {
        //TODO: Cache this information
        var publicItemsCount = await _itemService.GetPublicItemsCountByUser(profile.UserId);
        var averateRating = await _reviewService.GetAverageRatingForUser(profile.UserId);
        var totalReviews = await _reviewService.GetTotalReviewsCountForUser(profile.UserId);
        var borrowedItemsCount = await _loanService.GetBorrovedItemsCountByUser(profile.UserId);
        var lentItemsCount = await _loanService.GetLentItemsCountByUser(profile.UserId);

        return new ProfileAggregations
        {
            CountOfPublicItems = publicItemsCount,
            CountOfBorrowedItems = borrowedItemsCount,
            CountOfLentItems = lentItemsCount,
            AverageRating = averateRating,
            TotalReviews = totalReviews,
        };
    }
}
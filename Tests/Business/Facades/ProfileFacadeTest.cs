using AutoMapper;
using Moq;
using PujcovadloServer;
using PujcovadloServer.Authentication;
using PujcovadloServer.Business.Entities;
using PujcovadloServer.Business.Exceptions;
using PujcovadloServer.Business.Services;
using PujcovadloServer.Business.Services.Interfaces;
using PujcovadloServer.Requests;

namespace Tests.Business.Facades;

public class ProfileFacadeTest
{
    private ProfileFacade _profileFacade;
    private Mock<ProfileService> _profileService;
    private Mock<ItemService> _itemService;
    private Mock<ReviewService> _reviewService;
    private Mock<LoanService> _loanService;
    private Mock<IAuthenticateService> _authenticateService;
    private Mock<IMapper> _mapper;
    private Mock<PujcovadloServerConfiguration> _configuration;

    private ApplicationUser _user;

    private ApplicationUser _owner;

    private ApplicationUser _tenant;

    private PujcovadloServer.Business.Entities.Profile _userProfile;

    [SetUp]
    public void Setup()
    {
        _profileService = new Mock<ProfileService>(null);
        _itemService = new Mock<ItemService>(null);
        _reviewService = new Mock<ReviewService>(null);
        _loanService = new Mock<LoanService>(null, null);
        _authenticateService = new Mock<IAuthenticateService>();
        _mapper = new Mock<IMapper>();
        _configuration = new Mock<PujcovadloServerConfiguration>(null);

        _profileFacade = new ProfileFacade(_profileService.Object, _itemService.Object, _reviewService.Object,
            _loanService.Object, _authenticateService.Object, _mapper.Object, _configuration.Object);

        _user = new ApplicationUser() { Id = "1" };

        _owner = new ApplicationUser() { Id = "2" };

        _tenant = new ApplicationUser() { Id = "3" };

        _userProfile = new PujcovadloServer.Business.Entities.Profile
        {
            User = _owner,
            Description = "Popis profilu",
            ProfileImage = new Image { Id = 20 }
        };
    }

    #region GetProfile

    [Test]
    public async Task GetProfile_ProfileDoesNotExist_ThrowsException()
    {
        // Arrange
        var id = 1;
        _profileService
            .Setup(o => o.Get(id, true))
            .ThrowsAsync(new EntityNotFoundException());

        // Must throw ItemNotFoundException because the item does not exist
        Assert.ThrowsAsync<EntityNotFoundException>(async () => await _profileFacade.GetProfile(id));

        // Verify that GetById was called
        _profileService.Verify(o => o.Get(id, true), Times.Once);
    }

    [Test]
    public async Task GetProfile_ProfileExists_ReturnsItem()
    {
        // Arrange
        var id = 1;
        _userProfile.Id = id;

        _profileService
            .Setup(o => o.Get(id, true))
            .ReturnsAsync(_userProfile);

        // Must return the created item
        var result = await _profileFacade.GetProfile(id);

        // assert
        Assert.That(result.Id, Is.EqualTo(id));
        Assert.That(result.Description, Is.EqualTo(_userProfile.Description));
        Assert.That(result.User.Id, Is.EqualTo(_userProfile.User.Id));
        Assert.That(result.ProfileImage.Id, Is.EqualTo(_userProfile.ProfileImage.Id));

        // Verify that GetById was called
        _profileService.Verify(o => o.Get(id, true), Times.Once);
    }

    #endregion

    #region UpdateProfile

    [Test]
    public async Task UpdateProfile_UpdatesTheProfile_ReturnsItem()
    {
        var request = new ProfileUpdateRequest
        {
            Description = "Novy popis",
            Id = _userProfile.Id
        };

        var expectedProfile = new PujcovadloServer.Business.Entities.Profile
        {
            Id = _userProfile.Id,
            User = _owner,
            Description = request.Description,
            ProfileImage = new Image { Id = 20 }
        };

        // Must return the created item
        await _profileFacade.UpdateProfile(_userProfile, request);

        // assert
        Assert.That(_userProfile.Id, Is.EqualTo(expectedProfile.Id));
        Assert.That(_userProfile.Description, Is.EqualTo(expectedProfile.Description));
        Assert.That(_userProfile.User.Id, Is.EqualTo(expectedProfile.User.Id));
        Assert.That(_userProfile.ProfileImage.Id, Is.EqualTo(expectedProfile.ProfileImage.Id));

        // Verify that Update was called
        _profileService.Verify(o => o.Update(_userProfile), Times.Once);
    }

    #endregion
}
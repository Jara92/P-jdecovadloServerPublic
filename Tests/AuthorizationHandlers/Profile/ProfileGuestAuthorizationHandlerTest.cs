using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Moq;
using PujcovadloServer.Authentication;
using PujcovadloServer.AuthorizationHandlers.Profile;
using PujcovadloServer.Business.Enums;
using PujcovadloServer.Business.Services.Interfaces;

namespace Tests.AuthorizationHandlers.Profile;

public class ProfileGuestAuthorizationHandlerTest
{
    ProfileGuestAuthorizationHandler _authorizationHandler;
    Mock<IAuthenticateService> _authenticateService;

    /// <summary>
    /// Unauthenticated user which is completely anonymous.
    /// </summary>
    ClaimsPrincipal _unauthenticatedUser;

    private string? _unauthenticatedUserId = null;

    /// <summary>
    /// Authenticated user with the User role.
    /// </summary>
    ClaimsPrincipal _user;

    string _userId = "1";

    /// <summary>
    /// Authenticated user with the Owner role and is owner of the loan.
    /// </summary>
    ClaimsPrincipal _owner;

    private string _ownerId = "2";

    /// <summary>
    /// Authenticated user with the Tentant role and the tenant of the loan.
    /// </summary>
    ClaimsPrincipal _tenant;

    private string _tenantId = "3";

    /// <summary>
    /// Auhenticated user with the Admin role.
    /// </summary>
    ClaimsPrincipal _admin;

    string _adminId = "4";

    private ApplicationUser _targetUser;
    private PujcovadloServer.Business.Entities.Profile _userProfile;

    [SetUp]
    public void Setup()
    {
        _authenticateService = new Mock<IAuthenticateService>();
        _authorizationHandler = new ProfileGuestAuthorizationHandler(_authenticateService.Object);

        _unauthenticatedUser = new ClaimsPrincipal();
        _user = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
        {
            new Claim(ClaimTypes.Name, "ned.flanders"), new Claim(ClaimTypes.Sid, _userId),
            new Claim(ClaimTypes.Role, UserRoles.User)
        }));
        _owner = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
        {
            new Claim(ClaimTypes.Name, "marge.simpson"), new Claim(ClaimTypes.Sid, _ownerId),
            new Claim(ClaimTypes.Role, UserRoles.Owner), new Claim(ClaimTypes.Role, UserRoles.User)
        }));
        _tenant = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
        {
            new Claim(ClaimTypes.Name, "bart.simpson"), new Claim(ClaimTypes.Sid, _tenantId),
            new Claim(ClaimTypes.Role, UserRoles.Tenant), new Claim(ClaimTypes.Role, UserRoles.User)
        }));
        _admin = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
        {
            new Claim(ClaimTypes.Name, "homer.simpson"), new Claim(ClaimTypes.Sid, _adminId),
            new Claim(ClaimTypes.Role, UserRoles.Admin)
        }));

        _targetUser = new ApplicationUser
        {
            Id = _ownerId,
            UserName = "ned.flanders"
        };

        _userProfile = new PujcovadloServer.Business.Entities.Profile
        {
            User = _targetUser,
            Description = "Popis profilu"
        };
    }

    [Test]
    public async Task CreateUpdateDelete_UserIsGuest_ReturnsFalse()
    {
        var users = new List<ClaimsPrincipal> { _unauthenticatedUser, _user, _owner, _tenant, _admin };
        var requirements = new List<IAuthorizationRequirement>
        {
            ProfileOperations.Create, ProfileOperations.Update, ProfileOperations.Delete
        };

        foreach (var user in users)
        {
            // Mock user
            _authenticateService.Setup(x => x.TryGetCurrentUserId()).Returns(user.FindFirstValue(ClaimTypes.Sid));

            // act - every requirement should fail
            foreach (var requirement in requirements)
            {
                var context = new AuthorizationHandlerContext(new List<IAuthorizationRequirement> { requirement }, user,
                    _userProfile);
                await _authorizationHandler.HandleAsync(context);

                // None of these users can do anything with via this handler
                Assert.That(context.HasSucceeded, Is.False);
            }
        }
    }

    [Test]
    public async Task Read_UserIsGuest_ReturnsTrue()
    {
        // Mock user
        _authenticateService.Setup(x => x.TryGetCurrentUserId()).Returns(_owner.FindFirstValue(ClaimTypes.Sid));

        var context =
            new AuthorizationHandlerContext(new List<IAuthorizationRequirement> { ProfileOperations.Read }, _owner,
                _userProfile);
        await _authorizationHandler.HandleAsync(context);

        // Anyone can read a profile
        Assert.That(context.HasSucceeded, Is.True);
    }
}
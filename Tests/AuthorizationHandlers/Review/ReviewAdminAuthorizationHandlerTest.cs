using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Moq;
using PujcovadloServer.Authentication;
using PujcovadloServer.AuthorizationHandlers.ReturnProtocol;
using PujcovadloServer.AuthorizationHandlers.Review;
using PujcovadloServer.Business.Enums;
using PujcovadloServer.Business.Services.Interfaces;

namespace Tests.AuthorizationHandlers.Review;

public class ReviewAdminAuthorizationHandlerTest
{
    ReviewAdminAuthorizationHandler _authorizationHandler;
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

    private PujcovadloServer.Business.Entities.Item _item;

    private PujcovadloServer.Business.Entities.Loan _loan;

    private PujcovadloServer.Business.Entities.Review _review;


    [SetUp]
    public void Setup()
    {
        _authenticateService = new Mock<IAuthenticateService>();
        _authorizationHandler = new ReviewAdminAuthorizationHandler(_authenticateService.Object);

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

        _item = new PujcovadloServer.Business.Entities.Item
        {
            Owner = new ApplicationUser { Id = _ownerId }
        };

        _loan = new PujcovadloServer.Business.Entities.Loan
        {
            Item = _item,
            Tenant = new ApplicationUser { Id = _tenantId }
        };

        _review = new PujcovadloServer.Business.Entities.Review
        {
            Author = new ApplicationUser { Id = "20" },
            Comment = "All ok",
            Rating = 5
        };
    }

    #region NoAdminUsers

    [Test]
    public async Task HandleAsync_UserDoesNotHaveAdminRole_ReturnsFalse()
    {
        var users = new List<ClaimsPrincipal> { _unauthenticatedUser, _user, _owner, _tenant };
        var requirements = new List<IAuthorizationRequirement>
        {
            ReviewOperations.Read, ReviewOperations.Delete, ReviewOperations.Update
        };

        foreach (var user in users)
        {
            // Mock user
            _authenticateService.Setup(x => x.TryGetCurrentUserId()).Returns(user.FindFirstValue(ClaimTypes.Sid));

            // act - every requirement should fail
            foreach (var requirement in requirements)
            {
                var context = new AuthorizationHandlerContext(new List<IAuthorizationRequirement> { requirement }, user,
                    _review);
                await _authorizationHandler.HandleAsync(context);
                Assert.That(context.HasSucceeded, Is.False);
            }
        }
    }

    #endregion

    #region AdminUsers

    [Test]
    public async Task HandleAsync_UserHasAdminRole_ReturnsTrue()
    {
        var requirements = new List<IAuthorizationRequirement>
        {
            ReviewOperations.Read, ReviewOperations.Delete, ReviewOperations.Update
        };

        // Mock user
        _authenticateService.Setup(x => x.TryGetCurrentUserId()).Returns(_admin.FindFirstValue(ClaimTypes.Sid));

        // act - every requirement should fail
        foreach (var requirement in requirements)
        {
            var context =
                new AuthorizationHandlerContext(new List<IAuthorizationRequirement> { requirement }, _admin,
                    _review);
            await _authorizationHandler.HandleAsync(context);
            Assert.That(context.HasSucceeded, Is.True);
        }
    }

    #endregion
}
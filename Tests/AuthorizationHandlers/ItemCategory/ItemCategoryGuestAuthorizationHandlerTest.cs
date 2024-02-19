using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Moq;
using PujcovadloServer.AuthorizationHandlers.Item;
using PujcovadloServer.AuthorizationHandlers.ItemCategory;
using PujcovadloServer.Business.Enums;
using PujcovadloServer.Business.Services.Interfaces;

namespace Tests.AuthorizationHandlers.ItemCategory;

public class ItemCategoryGuestAuthorizationHandlerTest
{
    ItemCategoryGuestAuthorizationHandler _authorizationHandler;
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

    private PujcovadloServer.Business.Entities.ItemCategory _itemCategory;

    [SetUp]
    public void Setup()
    {
        _authenticateService = new Mock<IAuthenticateService>();
        _authorizationHandler = new ItemCategoryGuestAuthorizationHandler(_authenticateService.Object);

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

        _itemCategory = new PujcovadloServer.Business.Entities.ItemCategory
        {
            Id = 1,
            Name = "Test"
        };
    }

    [Test]
    public void Read_UserIsGuest_ShouldSucceed()
    {
        var users = new List<ClaimsPrincipal> { _unauthenticatedUser, _user, _owner, _tenant, _admin };

        foreach (var user in users)
        {
            // Mock the AuthenticateService to return the user
            _authenticateService.Setup(x => x.TryGetCurrentUserId()).Returns(user.FindFirstValue(ClaimTypes.Sid));

            var context = new AuthorizationHandlerContext(new[] { ItemCategoryOperations.Read }, user, _itemCategory);

            _authorizationHandler.HandleAsync(context);

            Assert.That(context.HasSucceeded, Is.True);
        }
    }

    [Test]
    public void CreateUpdateDelete_UserIsGuest_ShouldFail()
    {
        var users = new List<ClaimsPrincipal> { _unauthenticatedUser, _user, _owner, _tenant, _admin };
        var requirements = new List<IAuthorizationRequirement>
        {
            ItemCategoryOperations.Create, ItemCategoryOperations.Update, ItemCategoryOperations.Delete
        };

        foreach (var user in users)
        {
            // Mock the AuthenticateService to return the user
            _authenticateService.Setup(x => x.TryGetCurrentUserId()).Returns(user.FindFirstValue(ClaimTypes.Sid));

            // act - every requirement should fail
            foreach (var requirement in requirements)
            {
                var context = new AuthorizationHandlerContext(new[] { requirement }, user, _itemCategory);

                _authorizationHandler.HandleAsync(context);

                Assert.That(context.HasSucceeded, Is.False);
            }
        }
    }
}
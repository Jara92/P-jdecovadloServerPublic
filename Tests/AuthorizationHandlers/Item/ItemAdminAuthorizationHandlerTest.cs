using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Moq;
using PujcovadloServer.Authentication;
using PujcovadloServer.AuthorizationHandlers.Item;
using PujcovadloServer.Business.Enums;
using PujcovadloServer.Business.Services.Interfaces;
using static NUnit.Framework.Assert;

namespace Tests.AuthorizationHandlers.Item;

public class ItemAdminAuthorizationHandlerTest
{
    ItemAdminAuthorizationHandler _authorizationHandler;
    Mock<IAuthenticateService> _authenticateService;

    /// <summary>
    /// Unauthenticated user which is completely anonymous.
    /// </summary>
    ClaimsPrincipal _unauthenticatedUser;

    /// <summary>
    /// Authenticated user with the User role.
    /// </summary>
    ClaimsPrincipal _user;

    string _userId = "1";

    /// <summary>
    /// Authenticated user with the Owner role and is owner of the item.
    /// </summary>
    ClaimsPrincipal _owner;

    private string _ownerId = "2";

    /// <summary>
    /// Auhenticated user with the Admin role.
    /// </summary>
    ClaimsPrincipal _admin;

    string _adminId = "3";

    private PujcovadloServer.Business.Entities.Item _item;

    [SetUp]
    public void Setup()
    {
        _authenticateService = new Mock<IAuthenticateService>();
        _authorizationHandler = new ItemAdminAuthorizationHandler(_authenticateService.Object);

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
        _admin = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
        {
            new Claim(ClaimTypes.Name, "homer.simpson"), new Claim(ClaimTypes.Sid, _adminId),
            new Claim(ClaimTypes.Role, UserRoles.Admin)
        }));

        _item = new PujcovadloServer.Business.Entities.Item
        {
            Owner = new ApplicationUser { Id = _ownerId }
        };
    }

    [Test]
    public async Task CRUD_UserWithNoAdminRole_Fails()
    {
        // arrange
        var nonOwners = new List<ClaimsPrincipal> { _unauthenticatedUser, _user, _owner };

        // Requirements which should fail
        var requirements = new List<IAuthorizationRequirement>
        {
            ItemOperations.Read, ItemOperations.Update, ItemOperations.Delete, ItemOperations.Create,
            ItemOperations.CreateImage
        };

        foreach (var user in nonOwners)
        {
            // Mock the current user
            _authenticateService.Setup(x => x.TryGetCurrentUserId()).Returns(user.FindFirst(ClaimTypes.Sid)?.Value);

            // each of the requirements must be tested separately because we need all of them to fail
            foreach (var requirement in requirements)
            {
                // arrange
                var context = new AuthorizationHandlerContext(new List<IAuthorizationRequirement> { requirement },
                    user, _item);

                // act
                await _authorizationHandler.HandleAsync(context);

                // assert - This handler doest allow any of the requirements
                That(context.HasSucceeded, Is.False);
            }
        }
    }

    [Test]
    public async Task CRUD_UserIsAdmin_Succeeded()
    {
        // arrange - All statuses
        var statuses = new List<ItemStatus>
        {
            ItemStatus.Approving, ItemStatus.Denied, ItemStatus.Deleted, ItemStatus.Public
        };

        // arrange requirements
        var requirements = new List<IAuthorizationRequirement>
        {
            ItemOperations.Read, ItemOperations.Update, ItemOperations.Delete, ItemOperations.Create,
            ItemOperations.CreateImage
        };

        foreach (var status in statuses)
        {
            // arrange item status
            _item.Status = status;

            // Mock the current user
            _authenticateService.Setup(x => x.TryGetCurrentUserId()).Returns(_adminId);

            foreach (var requirement in requirements)
            {
                // arrange
                var context = new AuthorizationHandlerContext(new List<IAuthorizationRequirement> { requirement },
                    _admin, _item);

                // act
                await _authorizationHandler.HandleAsync(context);

                // assert - Admin can do anything no matter the status
                That(context.HasSucceeded, Is.True);
            }
        }
    }
}
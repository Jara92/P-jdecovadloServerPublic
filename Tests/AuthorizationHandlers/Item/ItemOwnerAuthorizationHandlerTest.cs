using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Moq;
using PujcovadloServer.Authentication;
using PujcovadloServer.AuthorizationHandlers.Item;
using PujcovadloServer.Business.Enums;
using PujcovadloServer.Business.Services.Interfaces;
using static NUnit.Framework.Assert;

namespace Tests.AuthorizationHandlers.Item;

public class ItemOwnerAuthorizationHandlerTest
{
    ItemOwnerAuthorizationHandler _authorizationHandler;
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
    /// Authenticated user with the Owner role.
    /// </summary>
    ClaimsPrincipal _owner;

    string _ownerId = "2";

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
        _authorizationHandler = new ItemOwnerAuthorizationHandler(_authenticateService.Object);

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
    public async Task Read_NonOwnerUsers_Fails()
    {
        // arrange
        var nonOwners = new List<ClaimsPrincipal> { _unauthenticatedUser, _user, _admin };

        // Requirements which should fail
        var requirements = new List<IAuthorizationRequirement>
            { ItemOperations.Read, ItemOperations.Update, ItemOperations.Delete, ItemOperations.Create };

        foreach (var user in nonOwners)
        {
            // Mock the current user
            _authenticateService.Setup(x => x.TryGetCurrentUserId()).Returns(user.FindFirst(ClaimTypes.Sid)?.Value);

            foreach (var requirement in requirements)
            {
                var context =
                    new AuthorizationHandlerContext(new List<IAuthorizationRequirement> { requirement }, user, _item);

                // act
                await _authorizationHandler.HandleAsync(context);

                // Everyone of these users can do nothing with the item within the context of this handler.
                That(context.HasSucceeded, Is.False);
            }
        }
    }

    #region Read

    [Test]
    public async Task Read_OwnerUser_Succeeds()
    {
        // Arrange statuses
        var statuses = new List<ItemStatus>
            { ItemStatus.Public, ItemStatus.Approving, ItemStatus.Deleted, ItemStatus.Denied };

        // Mock the current user
        _authenticateService.Setup(x => x.TryGetCurrentUserId()).Returns(_ownerId);

        foreach (var status in statuses)
        {
            // arrange status
            _item.Status = status;

            // arrange
            var context = new AuthorizationHandlerContext(new List<IAuthorizationRequirement> { ItemOperations.Read },
                _owner, _item);

            // act
            await _authorizationHandler.HandleAsync(context);

            // assert - User is the owner so he can read the item no matter the status
            That(context.HasSucceeded, Is.True);
        }
    }

    #endregion

    #region Create

    [Test]
    public async Task Create_OwnerUser_Fails()
    {
        // Mock the current user
        _authenticateService.Setup(x => x.TryGetCurrentUserId()).Returns(_ownerId);

        // arrange
        var context = new AuthorizationHandlerContext(new List<IAuthorizationRequirement> { ItemOperations.Create },
            _owner, _item);

        // act
        await _authorizationHandler.HandleAsync(context);

        // assert - Event the owner cannot create a new item using this handler
        That(context.HasSucceeded, Is.False);
    }

    #endregion

    #region Update

    [Test]
    public async Task Update_OwnerUserItemStatusDeleted_Fails()
    {
        // Mock the current user
        _authenticateService.Setup(x => x.TryGetCurrentUserId()).Returns(_ownerId);

        // arrange
        _item.Status = ItemStatus.Deleted;
        var context = new AuthorizationHandlerContext(new List<IAuthorizationRequirement> { ItemOperations.Update },
            _owner, _item);

        // act
        await _authorizationHandler.HandleAsync(context);

        // assert - Owner cannot update a deleted item
        That(context.HasSucceeded, Is.False);
    }

    [Test]
    public async Task Update_OwnerUserItemStatusNotDeleted_Succeeds()
    {
        // arrange statuses
        var statuses = new List<ItemStatus>
            { ItemStatus.Public, ItemStatus.Approving, ItemStatus.Denied };

        // Mock the current user
        _authenticateService.Setup(x => x.TryGetCurrentUserId()).Returns(_ownerId);

        foreach (var status in statuses)
        {
            // arrange status
            _item.Status = status;

            // arrange
            var context = new AuthorizationHandlerContext(new List<IAuthorizationRequirement> { ItemOperations.Update },
                _owner, _item);

            // act
            await _authorizationHandler.HandleAsync(context);

            // assert - Owner can update public, approving or denied item
            That(context.HasSucceeded, Is.True);
        }
    }

    #endregion

    #region Delete

    [Test]
    public async Task Delete_OwnerUserItemStatusDeleted_Fails()
    {
        // Mock the current user
        _authenticateService.Setup(x => x.TryGetCurrentUserId()).Returns(_ownerId);

        // arrange
        _item.Status = ItemStatus.Deleted;
        var context = new AuthorizationHandlerContext(new List<IAuthorizationRequirement> { ItemOperations.Delete },
            _owner, _item);

        // act
        await _authorizationHandler.HandleAsync(context);

        // assert - Owner cannot delete a deleted item
        That(context.HasSucceeded, Is.False);
    }

    [Test]
    public async Task Delete_OwnerUserItemStatusNotDeleted_Succeeds()
    {
        // arrange statuses
        var statuses = new List<ItemStatus>
            { ItemStatus.Public, ItemStatus.Approving, ItemStatus.Denied };

        // Mock the current user
        _authenticateService.Setup(x => x.TryGetCurrentUserId()).Returns(_ownerId);

        foreach (var status in statuses)
        {
            // arrange status
            _item.Status = status;

            // arrange
            var context = new AuthorizationHandlerContext(new List<IAuthorizationRequirement> { ItemOperations.Delete },
                _owner, _item);

            // act
            await _authorizationHandler.HandleAsync(context);

            // assert - Owner can delete public, approving or denied item
            That(context.HasSucceeded, Is.True);
        }
    }

    #endregion
}
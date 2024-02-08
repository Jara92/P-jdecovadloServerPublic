using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using PujcovadloServer.Authentication;
using PujcovadloServer.AuthorizationHandlers;
using PujcovadloServer.Business.Entities;
using PujcovadloServer.Business.Enums;
using PujcovadloServer.Business.Services.Interfaces;

namespace Tests.AuthorizationHandlers;

using Moq;

public class ItemAuthorizationHandlerTest
{
    ItemAuthorizationHandler _itemAuthorizationHandler;
    Mock<IAuthenticateService> _authenticateService;

    /// <summary>
    /// Unauthenticated user which is completely anonymous.
    /// </summary>
    ClaimsPrincipal _unauthenticatedUser;

    /// <summary>
    /// Authenticated user with the User role.
    /// </summary>
    ClaimsPrincipal _user;

    /// <summary>
    /// Authenticated user with the Owner role.
    /// </summary>
    ClaimsPrincipal _owner;

    /// <summary>
    /// Auhenticated user with the Admin role.
    /// </summary>
    ClaimsPrincipal _admin;

    [SetUp]
    public void Setup()
    {
        _authenticateService = new Mock<IAuthenticateService>();
        _itemAuthorizationHandler = new ItemAuthorizationHandler(_authenticateService.Object);

        _unauthenticatedUser = new ClaimsPrincipal();
        _user = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
        {
            new Claim(ClaimTypes.Name, "ned.flanders"), new Claim(ClaimTypes.Sid, "1"),
            new Claim(ClaimTypes.Role, UserRoles.User)
        }));
        _owner = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
        {
            new Claim(ClaimTypes.Name, "marge.simpson"), new Claim(ClaimTypes.Sid, "2"),
            new Claim(ClaimTypes.Role, UserRoles.Owner), new Claim(ClaimTypes.Role, UserRoles.User)
        }));
        _admin = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
        {
            new Claim(ClaimTypes.Name, "homer.simpson"), new Claim(ClaimTypes.Sid, "3"),
            new Claim(ClaimTypes.Role, UserRoles.Admin)
        }));
    }

    #region readTests

    [Test]
    public void ReadItem_UnauthenticatedUserPublicItem_ShouldSuccess()
    {
        // Arrange
        var item = new Item { Id = 1, Name = "Test item", Status = ItemStatus.Public };
        var requirement = ItemAuthorizationHandler.Operations.Read;

        var authzContext = new AuthorizationHandlerContext(new List<IAuthorizationRequirement> { requirement },
            _unauthenticatedUser, item);

        // Act
        _itemAuthorizationHandler.HandleAsync(authzContext).Wait();

        // Assert
        Assert.True(authzContext.HasSucceeded);
    }

    [Test]
    public void ReadItem_UnauthenticatedUserNonPublicItem_ShouldFail()
    {
        // Arrange
        var itemApproving = new Item { Id = 1, Name = "Test item", Status = ItemStatus.Approving };
        var itemDeleted = new Item { Id = 2, Name = "Test item", Status = ItemStatus.Deleted };
        var itemDenied = new Item { Id = 3, Name = "Test item", Status = ItemStatus.Denied };

        var items = new List<Item> { itemApproving, itemDeleted, itemDenied };
        var requirement = ItemAuthorizationHandler.Operations.Read;

        foreach (var item in items)
        {
            var authzContext = new AuthorizationHandlerContext(new List<IAuthorizationRequirement> { requirement },
                _unauthenticatedUser, item);

            // Act
            _itemAuthorizationHandler.HandleAsync(authzContext).Wait();

            // Assert
            Assert.False(authzContext.HasSucceeded);
        }
    }

    [Test]
    public void ReadItem_AuthenticatedUserPublicItem_ShouldSuccess()
    {
        // Arrange
        var item = new Item { Id = 1, Name = "Test item", Status = ItemStatus.Public };
        var requirement = ItemAuthorizationHandler.Operations.Read;

        var authzContext =
            new AuthorizationHandlerContext(new List<IAuthorizationRequirement> { requirement }, _user, item);

        // Act
        _itemAuthorizationHandler.HandleAsync(authzContext).Wait();

        // Assert
        Assert.True(authzContext.HasSucceeded);
    }

    [Test]
    public void ReadItem_AuthenticatedUserNonPublicItem_ShouldFail()
    {
        // Arrange
        var itemApproving = new Item { Id = 1, Name = "Test item", Status = ItemStatus.Approving };
        var itemDeleted = new Item { Id = 2, Name = "Test item", Status = ItemStatus.Deleted };
        var itemDenied = new Item { Id = 3, Name = "Test item", Status = ItemStatus.Denied };

        var items = new List<Item> { itemApproving, itemDeleted, itemDenied };
        var requirement = ItemAuthorizationHandler.Operations.Read;

        foreach (var item in items)
        {
            var authzContext =
                new AuthorizationHandlerContext(new List<IAuthorizationRequirement> { requirement }, _user, item);

            // Act
            _itemAuthorizationHandler.HandleAsync(authzContext).Wait();

            // Assert
            Assert.False(authzContext.HasSucceeded);
        }
    }

    [Test]
    public void ReadItem_UserIsOwnerItemIsNotDeleted_ShouldSuccess()
    {
        var user = new ApplicationUser { Id = "2" };
        var itemPublic = new Item { Id = 1, Name = "Test item", Status = ItemStatus.Public, Owner = user };
        var itemApproving = new Item { Id = 2, Name = "Test item", Status = ItemStatus.Approving, Owner = user };
        var itemDenied = new Item { Id = 3, Name = "Test item", Status = ItemStatus.Denied, Owner = user };

        // return user id
        _authenticateService.Setup(a => a.TryGetCurrentUserId()).Returns(user.Id);
        _authenticateService.Setup(a => a.GetCurrentUser()).Returns(Task.FromResult(user));

        // Arrange
        var requirement = ItemAuthorizationHandler.Operations.Read;

        foreach (var item in new List<Item> { itemPublic, itemApproving, itemDenied })
        {
            var authzContext =
                new AuthorizationHandlerContext(new List<IAuthorizationRequirement> { requirement }, _owner, item);

            // Act
            _itemAuthorizationHandler.HandleAsync(authzContext).Wait();

            // Assert
            Assert.True(authzContext.HasSucceeded);
        }
    }

    [Test]
    public void ReadItem_UserIsOwnerItemIsDeleted_ShouldFail()
    {
        var user = new ApplicationUser { Id = "2" };
        var itemDeleted = new Item { Id = 1, Name = "Test item", Status = ItemStatus.Deleted, Owner = user };

        // return user id
        _authenticateService.Setup(a => a.TryGetCurrentUserId()).Returns(user.Id);
        _authenticateService.Setup(a => a.GetCurrentUser()).Returns(Task.FromResult(user));

        // Arrange
        var requirement = ItemAuthorizationHandler.Operations.Read;

        var authzContext =
            new AuthorizationHandlerContext(new List<IAuthorizationRequirement> { requirement }, _owner, itemDeleted);

        // Act
        _itemAuthorizationHandler.HandleAsync(authzContext).Wait();

        // Assert
        Assert.False(authzContext.HasSucceeded);
    }

    #endregion

    #region createTests

    [Test]
    public void CreateItem_UnauthenticatedUser_ShouldFail()
    {
        // Arrange
        var item = new Item { Id = 1, Name = "Test item", Status = ItemStatus.Public };
        var requirement = ItemAuthorizationHandler.Operations.Create;

        var authzContext = new AuthorizationHandlerContext(new List<IAuthorizationRequirement> { requirement },
            _unauthenticatedUser, item);

        // Act
        _itemAuthorizationHandler.HandleAsync(authzContext).Wait();

        // Assert
        Assert.False(authzContext.HasSucceeded);
    }

    [Test]
    public void CreateItem_AuthenticatedUser_ShouldFail()
    {
        // Arrange
        var item = new Item { Id = 1, Name = "Test item", Status = ItemStatus.Public };
        var requirement = ItemAuthorizationHandler.Operations.Create;

        var authzContext = new AuthorizationHandlerContext(new List<IAuthorizationRequirement> { requirement },
            _user, item);

        // Act
        _itemAuthorizationHandler.HandleAsync(authzContext).Wait();

        // Should fail because the user has not the Owner role
        Assert.False(authzContext.HasSucceeded);
    }

    [Test]
    public void CreateItem_UserIsOwner_ShouldSuccess()
    {
        // Arrange
        var item = new Item { Id = 1, Name = "Test item", Status = ItemStatus.Public };
        var requirement = ItemAuthorizationHandler.Operations.Create;

        var authzContext = new AuthorizationHandlerContext(new List<IAuthorizationRequirement> { requirement },
            _owner, item);

        // Act
        _itemAuthorizationHandler.HandleAsync(authzContext).Wait();

        // Assert
        Assert.True(authzContext.HasSucceeded);
    }

    #endregion

    #region updateTests

    [Test]
    public void UpdateItem_UnauthenticatedUser_ShouldFail()
    {
        // Arrange
        var item = new Item { Id = 1, Name = "Test item", Status = ItemStatus.Public };
        var requirement = ItemAuthorizationHandler.Operations.Update;

        var authzContext = new AuthorizationHandlerContext(new List<IAuthorizationRequirement> { requirement },
            _unauthenticatedUser, item);

        // Act
        _itemAuthorizationHandler.HandleAsync(authzContext).Wait();

        // Assert
        Assert.False(authzContext.HasSucceeded);
    }

    [Test]
    public void UpdateItem_AuthenticatedUser_ShouldFail()
    {
        // Arrange
        var item = new Item { Id = 1, Name = "Test item", Status = ItemStatus.Public };
        var requirement = ItemAuthorizationHandler.Operations.Update;

        var authzContext = new AuthorizationHandlerContext(new List<IAuthorizationRequirement> { requirement },
            _user, item);

        // Act
        _itemAuthorizationHandler.HandleAsync(authzContext).Wait();

        // Should fail because the user is not the owner
        Assert.False(authzContext.HasSucceeded);
    }

    [Test]
    public void UpdateItem_UserIsOwner_ShouldSuccess()
    {
        var user = new ApplicationUser { Id = "2" };

        // return user id
        _authenticateService.Setup(a => a.TryGetCurrentUserId()).Returns(user.Id);
        _authenticateService.Setup(a => a.GetCurrentUser()).Returns(Task.FromResult(user));

        // Arrange
        var item = new Item { Id = 1, Name = "Test item", Status = ItemStatus.Public, Owner = user };
        var requirement = ItemAuthorizationHandler.Operations.Update;

        var authzContext = new AuthorizationHandlerContext(new List<IAuthorizationRequirement> { requirement },
            _owner, item);

        // Act
        _itemAuthorizationHandler.HandleAsync(authzContext).Wait();

        // Assert
        Assert.True(authzContext.HasSucceeded);
    }

    [Test]
    public void UpdateItem_UserIsOwnerButItemIsDeleted_ShouldFail()
    {
        var user = new ApplicationUser { Id = "2" };

        // return user id
        _authenticateService.Setup(a => a.TryGetCurrentUserId()).Returns(user.Id);
        _authenticateService.Setup(a => a.GetCurrentUser()).Returns(Task.FromResult(user));

        // Arrange
        var item = new Item { Id = 1, Name = "Test item", Status = ItemStatus.Deleted, Owner = user };
        var requirement = ItemAuthorizationHandler.Operations.Update;

        var authzContext = new AuthorizationHandlerContext(new List<IAuthorizationRequirement> { requirement },
            _owner, item);

        // Act
        _itemAuthorizationHandler.HandleAsync(authzContext).Wait();

        // Should fail because deleted item cannot be updated by the owner
        Assert.False(authzContext.HasSucceeded);
    }

    #endregion

    #region deleteTests

    [Test]
    public void DeleteItem_UnauthenticatedUser_ShouldFail()
    {
        // Arrange
        var item = new Item { Id = 1, Name = "Test item", Status = ItemStatus.Public };
        var requirement = ItemAuthorizationHandler.Operations.Delete;

        var authzContext = new AuthorizationHandlerContext(new List<IAuthorizationRequirement> { requirement },
            _unauthenticatedUser, item);

        // Act
        _itemAuthorizationHandler.HandleAsync(authzContext).Wait();

        // Assert
        Assert.False(authzContext.HasSucceeded);
    }

    [Test]
    public void DeleteItem_AuthenticatedUser_ShouldFail()
    {
        // Arrange
        var item = new Item { Id = 1, Name = "Test item", Status = ItemStatus.Public };
        var requirement = ItemAuthorizationHandler.Operations.Delete;

        var authzContext = new AuthorizationHandlerContext(new List<IAuthorizationRequirement> { requirement },
            _user, item);

        // Act
        _itemAuthorizationHandler.HandleAsync(authzContext).Wait();

        // Should fail because the user is not the owner
        Assert.False(authzContext.HasSucceeded);
    }

    [Test]
    public void DeleteItem_UserIsOwner_ShouldSuccess()
    {
        var user = new ApplicationUser { Id = "2" };

        // return user id
        _authenticateService.Setup(a => a.TryGetCurrentUserId()).Returns(user.Id);
        _authenticateService.Setup(a => a.GetCurrentUser()).Returns(Task.FromResult(user));

        // Arrange
        var item = new Item { Id = 1, Name = "Test item", Status = ItemStatus.Public, Owner = user };
        var requirement = ItemAuthorizationHandler.Operations.Delete;

        var authzContext = new AuthorizationHandlerContext(new List<IAuthorizationRequirement> { requirement },
            _owner, item);

        // Act
        _itemAuthorizationHandler.HandleAsync(authzContext).Wait();

        // Assert
        Assert.True(authzContext.HasSucceeded);
    }

    [Test]
    public void DeleteItem_UserIsOwnerButItemIsDeleted_ShouldFail()
    {
        var user = new ApplicationUser { Id = "2" };

        // return user id
        _authenticateService.Setup(a => a.TryGetCurrentUserId()).Returns(user.Id);
        _authenticateService.Setup(a => a.GetCurrentUser()).Returns(Task.FromResult(user));

        // Arrange
        var item = new Item { Id = 1, Name = "Test item", Status = ItemStatus.Deleted, Owner = user };
        var requirement = ItemAuthorizationHandler.Operations.Delete;

        var authzContext = new AuthorizationHandlerContext(new List<IAuthorizationRequirement> { requirement },
            _owner, item);

        // Act
        _itemAuthorizationHandler.HandleAsync(authzContext).Wait();

        // Should fail because deleted item cannot be deleted by the owner again
        Assert.False(authzContext.HasSucceeded);
    }

    #endregion

    #region adminTests

    [Test]
    public void DoCrud_IsAdmin_ShouldSuccess()
    {
        // Arrange
        var itemPublic = new Item { Id = 1, Name = "Test item", Status = ItemStatus.Public };
        var itemApproving = new Item { Id = 2, Name = "Test item", Status = ItemStatus.Approving };
        var itemDenied = new Item { Id = 3, Name = "Test item", Status = ItemStatus.Denied };
        var itemDeleted = new Item { Id = 4, Name = "Test item", Status = ItemStatus.Deleted };

        var requirements = new List<IAuthorizationRequirement>
        {
            ItemAuthorizationHandler.Operations.Create,
            ItemAuthorizationHandler.Operations.Read,
            ItemAuthorizationHandler.Operations.Update,
            ItemAuthorizationHandler.Operations.Delete
        };

        foreach (var item in new List<Item> { itemPublic, itemApproving, itemDenied, itemDeleted })
        {
            var authzContext = new AuthorizationHandlerContext(requirements, _admin, item);

            // Act
            _itemAuthorizationHandler.HandleAsync(authzContext).Wait();

            // Assert
            Assert.True(authzContext.HasSucceeded);
        }
    }

    #endregion
}
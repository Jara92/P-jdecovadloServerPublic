using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using PujcovadloServer.AuthorizationHandlers;
using PujcovadloServer.Business.Entities;
using PujcovadloServer.Business.Enums;
using PujcovadloServer.Business.Services.Interfaces;

namespace Tests.AuthorizationHandlers;

using Moq;

public class ItemCategoryAuthorizationHandlerTest
{
    ItemCategoryAuthorizationHandler _itemCategoryAuthorizationHandler;
    Mock<IAuthenticateService> _authenticateService;

    /// <summary>
    /// Unauthenticated user which is not logged in.
    /// </summary>
    ClaimsPrincipal _unauthenticatedUser;
    
    /// <summary>
    /// Authenticated user with role User.
    /// </summary>
    ClaimsPrincipal _user;
    
    /// <summary>
    /// Authenticated user with role Admin.
    /// </summary>
    ClaimsPrincipal _admin;

    [SetUp]
    public void Setup()
    {
        _authenticateService = new Mock<IAuthenticateService>();
        _itemCategoryAuthorizationHandler = new ItemCategoryAuthorizationHandler(_authenticateService.Object);

        _unauthenticatedUser = new ClaimsPrincipal();
        _user = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
        {
            new Claim(ClaimTypes.Name, "ned.flanders"), new Claim(ClaimTypes.Sid, "1"),
            new Claim(ClaimTypes.Role, UserRoles.User)
        }));
        _admin = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
        {
            new Claim(ClaimTypes.Name, "homer.simpson"), new Claim(ClaimTypes.Sid, "2"),
            new Claim(ClaimTypes.Role, UserRoles.Admin)
        }));
    }

    [Test]
    public void ReadCategory_UnauthenticatedUser_ShouldSuccess()
    {
        // Arrange
        var itemCategory = new ItemCategory { Id = 1, Name = "Test category" };
        var requirement = ItemCategoryAuthorizationHandler.Operations.Read;

        var authzContext = new AuthorizationHandlerContext(new List<IAuthorizationRequirement> { requirement },
            _unauthenticatedUser, itemCategory);

        // Act
        _itemCategoryAuthorizationHandler.HandleAsync(authzContext).Wait();

        // Assert
        Assert.True(authzContext.HasSucceeded);
    }

    [Test]
    public void ReadCategory_AuthenticatedUser_ShouldSuccess()
    {
        // Arrange
        var itemCategory = new ItemCategory { Id = 1, Name = "Test category" };
        var requirement = ItemCategoryAuthorizationHandler.Operations.Read;

        var authzContext =
            new AuthorizationHandlerContext(new List<IAuthorizationRequirement> { requirement }, _user, itemCategory);

        // Act
        _itemCategoryAuthorizationHandler.HandleAsync(authzContext).Wait();

        // Assert
        Assert.True(authzContext.HasSucceeded);
    }

    [Test]
    public void CreateCategory_UnauthenticatedUser_ShouldFail()
    {
        // Arrange
        var itemCategory = new ItemCategory { Id = 1, Name = "Test category" };
        var requirement = ItemCategoryAuthorizationHandler.Operations.Create;

        var authzContext = new AuthorizationHandlerContext(new List<IAuthorizationRequirement> { requirement },
            _unauthenticatedUser, itemCategory);

        // Act
        _itemCategoryAuthorizationHandler.HandleAsync(authzContext).Wait();

        // Assert
        Assert.False(authzContext.HasSucceeded);
    }

    [Test]
    public void CreateCategory_AuthenticatedUser_ShouldFail()
    {
        // Arrange
        var itemCategory = new ItemCategory { Id = 1, Name = "Test category" };
        var requirement = ItemCategoryAuthorizationHandler.Operations.Create;

        var authzContext =
            new AuthorizationHandlerContext(new List<IAuthorizationRequirement> { requirement }, _user, itemCategory);

        // Act
        _itemCategoryAuthorizationHandler.HandleAsync(authzContext).Wait();

        // Assert
        Assert.False(authzContext.HasSucceeded);
    }

    [Test]
    public void UpdateCategory_UnauthenticatedUser_ShouldFail()
    {
        // Arrange
        var itemCategory = new ItemCategory { Id = 1, Name = "Test category" };
        var requirement = ItemCategoryAuthorizationHandler.Operations.Update;

        var authzContext = new AuthorizationHandlerContext(new List<IAuthorizationRequirement> { requirement },
            _unauthenticatedUser, itemCategory);

        // Act
        _itemCategoryAuthorizationHandler.HandleAsync(authzContext).Wait();

        // Assert
        Assert.False(authzContext.HasSucceeded);
    }

    [Test]
    public void UpdateCategory_AuthenticatedUser_ShouldFail()
    {
        // Arrange
        var itemCategory = new ItemCategory { Id = 1, Name = "Test category" };
        var requirement = ItemCategoryAuthorizationHandler.Operations.Update;

        var authzContext =
            new AuthorizationHandlerContext(new List<IAuthorizationRequirement> { requirement }, _user, itemCategory);

        // Act
        _itemCategoryAuthorizationHandler.HandleAsync(authzContext).Wait();

        // Assert
        Assert.False(authzContext.HasSucceeded);
    }

    [Test]
    public void DeleteCategory_UnauthenticatedUser_ShouldFail()
    {
        // Arrange
        var itemCategory = new ItemCategory { Id = 1, Name = "Test category" };
        var requirement = ItemCategoryAuthorizationHandler.Operations.Delete;

        var authzContext = new AuthorizationHandlerContext(new List<IAuthorizationRequirement> { requirement },
            _unauthenticatedUser, itemCategory);

        // Act
        _itemCategoryAuthorizationHandler.HandleAsync(authzContext).Wait();

        // Assert
        Assert.False(authzContext.HasSucceeded);
    }

    [Test]
    public void DeleteCategory_AuthenticatedUser_ShouldFail()
    {
        // Arrange
        var itemCategory = new ItemCategory { Id = 1, Name = "Test category" };
        var requirement = ItemCategoryAuthorizationHandler.Operations.Delete;

        var authzContext =
            new AuthorizationHandlerContext(new List<IAuthorizationRequirement> { requirement }, _user, itemCategory);

        // Act
        _itemCategoryAuthorizationHandler.HandleAsync(authzContext).Wait();

        // Assert
        Assert.False(authzContext.HasSucceeded);
    }

    [Test]
    public void DoCrud_IsAdmin_ShouldSuccess()
    {
        // Arrange
        var itemCategory = new ItemCategory { Id = 1, Name = "Test category" };
        var requirements = new List<IAuthorizationRequirement>
        {
            ItemCategoryAuthorizationHandler.Operations.Create,
            ItemCategoryAuthorizationHandler.Operations.Read,
            ItemCategoryAuthorizationHandler.Operations.Update,
            ItemCategoryAuthorizationHandler.Operations.Delete
        };

        var authzContext = new AuthorizationHandlerContext(requirements, _admin, itemCategory);

        // Act
        _itemCategoryAuthorizationHandler.HandleAsync(authzContext).Wait();

        // Assert
        Assert.True(authzContext.HasSucceeded);
    }
}
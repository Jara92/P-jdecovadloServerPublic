using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Moq;
using PujcovadloServer.Authentication;
using PujcovadloServer.AuthorizationHandlers;
using PujcovadloServer.Business.Entities;
using PujcovadloServer.Business.Enums;
using PujcovadloServer.Business.Services.Interfaces;

namespace Tests.AuthorizationHandlers;

public class ImageAuthorizationHandlerTest
{
    ImageAuthorizationHandler _imageAuthorizationHandler;
    Mock<IAuthenticateService> _authenticateService;

    /// <summary>
    /// Unauthenticated user which is completely anonymous.
    /// </summary>
    ClaimsPrincipal _unauthenticatedUser;

    /// <summary>
    /// Authenticated user with the User role.
    /// </summary>
    ClaimsPrincipal _user;

    string userId = 1.ToString();

    /// <summary>
    /// Authenticated user with the Owner role.
    /// </summary>
    ClaimsPrincipal _owner;

    string ownerId = 2.ToString();

    /// <summary>
    /// Authenticated user with the Tenant role.
    /// </summary>
    ClaimsPrincipal _tenant;

    string tenantId = 3.ToString();

    /// <summary>
    /// Auhenticated user with the Admin role.
    /// </summary>
    ClaimsPrincipal _admin;

    string adminId = 4.ToString();

    Image _image = default!;

    Loan _loan = default!;

    Item _item = default!;

    PickupProtocol _pickupProtocol = default!;

    ReturnProtocol _returnProtocol = default!;

    [SetUp]
    public void Setup()
    {
        _authenticateService = new Mock<IAuthenticateService>();
        _imageAuthorizationHandler = new ImageAuthorizationHandler(_authenticateService.Object);

        _unauthenticatedUser = new ClaimsPrincipal();
        _user = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
        {
            new Claim(ClaimTypes.Name, "ned.flanders"), new Claim(ClaimTypes.Sid, userId),
            new Claim(ClaimTypes.Role, UserRoles.User)
        }));
        _owner = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
        {
            new Claim(ClaimTypes.Name, "marge.simpson"), new Claim(ClaimTypes.Sid, ownerId),
            new Claim(ClaimTypes.Role, UserRoles.Owner), new Claim(ClaimTypes.Role, UserRoles.User)
        }));
        _tenant = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
        {
            new Claim(ClaimTypes.Name, "bart.simpson"), new Claim(ClaimTypes.Sid, tenantId),
            new Claim(ClaimTypes.Role, UserRoles.Tenant), new Claim(ClaimTypes.Role, UserRoles.User)
        }));
        _admin = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
        {
            new Claim(ClaimTypes.Name, "homer.simpson"), new Claim(ClaimTypes.Sid, adminId),
            new Claim(ClaimTypes.Role, UserRoles.Admin)
        }));

        _image = new Image
        {
            Owner = new ApplicationUser { Id = ownerId }
        };

        _item = new Item { Status = ItemStatus.Public, Owner = new ApplicationUser { Id = ownerId } };

        _loan = new Loan
        {
            Item = new Item { Owner = new ApplicationUser { Id = ownerId } },
            Tenant = new ApplicationUser { Id = tenantId }
        };

        _pickupProtocol = new PickupProtocol { Loan = _loan };
        _returnProtocol = new ReturnProtocol { Loan = _loan };
    }

    #region Read

    [Test]
    public async Task Read_UnauthenticatedUserImageBelongsToPublicItem_ReturnsSuccess()
    {
        // Arrange
        _image.Item = _item;

        var requirement = ItemAuthorizationHandler.Operations.Read;

        // Mock current user
        _authenticateService.Setup(x => x.TryGetCurrentUserId()).Returns((string?)null);

        var authzContext = new AuthorizationHandlerContext(new List<IAuthorizationRequirement> { requirement },
            _unauthenticatedUser, _image);

        // Act
        _imageAuthorizationHandler.HandleAsync(authzContext).Wait();

        // Assert
        Assert.True(authzContext.HasSucceeded);
    }

    [Test]
    public async Task Read_UnauthenticatedUserImageBelongsToPrivateItem_ReturnsFailure()
    {
        // Arrange
        _image.Item = _item;
        var statuses = new List<ItemStatus> { ItemStatus.Approving, ItemStatus.Denied, ItemStatus.Deleted };

        var requirement = ItemAuthorizationHandler.Operations.Read;

        // Mock current user
        _authenticateService.Setup(x => x.TryGetCurrentUserId()).Returns((string?)null);

        foreach (var status in statuses)
        {
            _image.Item.Status = status;
            var authzContext = new AuthorizationHandlerContext(new List<IAuthorizationRequirement> { requirement },
                _unauthenticatedUser, _image);

            // Act
            _imageAuthorizationHandler.HandleAsync(authzContext).Wait();

            // Assert
            Assert.False(authzContext.HasSucceeded);
        }
    }

    [Test]
    public async Task Read_AuthenticatedUserImageBelongsToPrivateItem_ReturnsFailure()
    {
        // Arrange
        _image.Item = _item;
        var statuses = new List<ItemStatus> { ItemStatus.Approving, ItemStatus.Denied, ItemStatus.Deleted };

        var requirement = ItemAuthorizationHandler.Operations.Read;

        // Mock current user
        _authenticateService.Setup(x => x.TryGetCurrentUserId()).Returns(userId);

        foreach (var status in statuses)
        {
            _image.Item.Status = status;
            var authzContext = new AuthorizationHandlerContext(new List<IAuthorizationRequirement> { requirement },
                _user, _image);

            // Act
            _imageAuthorizationHandler.HandleAsync(authzContext).Wait();

            // Assert - even if the user is authenticated, he cannot read the image if the item is not public
            Assert.False(authzContext.HasSucceeded);
        }
    }

    [Test]
    public void Read_UserIsItemOwner_ReturnsSuccess()
    {
        // Arrange
        _image.Item = _item;
        var statuses = new List<ItemStatus> { ItemStatus.Approving, ItemStatus.Denied, ItemStatus.Public };

        var requirement = ItemAuthorizationHandler.Operations.Read;

        // Mock current user
        _authenticateService.Setup(x => x.TryGetCurrentUserId()).Returns(ownerId);

        foreach (var status in statuses)
        {
            var authzContext = new AuthorizationHandlerContext(new List<IAuthorizationRequirement> { requirement },
                _owner, _image);

            _image.Item.Status = status;
            // Act
            _imageAuthorizationHandler.HandleAsync(authzContext).Wait();

            // Assert - Owner can see the image if the item is not deleted
            Assert.True(authzContext.HasSucceeded);
        }

        var authzContext2 = new AuthorizationHandlerContext(new List<IAuthorizationRequirement> { requirement },
            _owner, _image);

        // Cant see deleted item
        _image.Item.Status = ItemStatus.Deleted;
        _imageAuthorizationHandler.HandleAsync(authzContext2).Wait();
        Assert.False(authzContext2.HasSucceeded);
    }

    [Test]
    public void Read_UnauthenticatedUserImageBelongsToPickupProtocol_ReturnsFailure()
    {
        _image.PickupProtocol = _pickupProtocol;

        var requirement = ItemAuthorizationHandler.Operations.Read;

        // Mock current user
        _authenticateService.Setup(x => x.TryGetCurrentUserId()).Returns((string?)null);

        var authzContext = new AuthorizationHandlerContext(new List<IAuthorizationRequirement> { requirement },
            _unauthenticatedUser, _image);

        // Act
        _imageAuthorizationHandler.HandleAsync(authzContext).Wait();

        // Assert - unauthorized user cannot see pickup protocol image
        Assert.False(authzContext.HasSucceeded);
    }

    [Test]
    public void Read_AuthenticatedUserImageBelongsToPickupProtocol_ReturnsFailure()
    {
        _image.PickupProtocol = _pickupProtocol;

        var requirement = ItemAuthorizationHandler.Operations.Read;

        // Mock current user
        _authenticateService.Setup(x => x.TryGetCurrentUserId()).Returns(userId);

        var authzContext = new AuthorizationHandlerContext(new List<IAuthorizationRequirement> { requirement },
            _user, _image);

        // Act
        _imageAuthorizationHandler.HandleAsync(authzContext).Wait();

        // Assert - the user has nothing to do with the pickup protocol so he cant see the image
        Assert.False(authzContext.HasSucceeded);
    }

    [Test]
    public void Read_UserIsTenantAndItemBelongsToPickupProtocol_ReturnsSuccess()
    {
        _image.PickupProtocol = _pickupProtocol;

        var requirement = ItemAuthorizationHandler.Operations.Read;

        // Mock current user
        _authenticateService.Setup(x => x.TryGetCurrentUserId()).Returns(tenantId);

        var authzContext = new AuthorizationHandlerContext(new List<IAuthorizationRequirement> { requirement },
            _tenant, _image);

        // Act
        _imageAuthorizationHandler.HandleAsync(authzContext).Wait();

        // Assert - tenant can see the image
        Assert.True(authzContext.HasSucceeded);
    }

    [Test]
    public void Read_UserIsOwnerAndItemBelongsToPickupProtocol_ReturnsSuccess()
    {
        _image.PickupProtocol = _pickupProtocol;

        var requirement = ItemAuthorizationHandler.Operations.Read;

        // Mock current user
        _authenticateService.Setup(x => x.TryGetCurrentUserId()).Returns(ownerId);

        var authzContext = new AuthorizationHandlerContext(new List<IAuthorizationRequirement> { requirement },
            _owner, _image);

        // Act
        _imageAuthorizationHandler.HandleAsync(authzContext).Wait();

        // Assert - tenant can see the image
        Assert.True(authzContext.HasSucceeded);
    }

    [Test]
    public void Read_UnauthenticatedUserImageBelongsToReturnProtocol_ReturnsFailure()
    {
        _image.ReturnProtocol = _returnProtocol;

        var requirement = ItemAuthorizationHandler.Operations.Read;

        // Mock current user
        _authenticateService.Setup(x => x.TryGetCurrentUserId()).Returns((string?)null);

        var authzContext = new AuthorizationHandlerContext(new List<IAuthorizationRequirement> { requirement },
            _unauthenticatedUser, _image);

        // Act
        _imageAuthorizationHandler.HandleAsync(authzContext).Wait();

        // Assert - unauthorized user cannot see pickup protocol image
        Assert.False(authzContext.HasSucceeded);
    }

    [Test]
    public void Read_AuthenticatedUserImageBelongsToReturnProtocol_ReturnsFailure()
    {
        _image.ReturnProtocol = _returnProtocol;

        var requirement = ItemAuthorizationHandler.Operations.Read;

        // Mock current user
        _authenticateService.Setup(x => x.TryGetCurrentUserId()).Returns(userId);

        var authzContext = new AuthorizationHandlerContext(new List<IAuthorizationRequirement> { requirement },
            _user, _image);

        // Act
        _imageAuthorizationHandler.HandleAsync(authzContext).Wait();

        // Assert - the user has nothing to do with the pickup protocol so he cant see the image
        Assert.False(authzContext.HasSucceeded);
    }

    [Test]
    public void Read_UserIsTenantAndItemBelongsToReturnProtocol_ReturnsSuccess()
    {
        _image.ReturnProtocol = _returnProtocol;

        var requirement = ItemAuthorizationHandler.Operations.Read;

        // Mock current user
        _authenticateService.Setup(x => x.TryGetCurrentUserId()).Returns(tenantId);

        var authzContext = new AuthorizationHandlerContext(new List<IAuthorizationRequirement> { requirement },
            _tenant, _image);

        // Act
        _imageAuthorizationHandler.HandleAsync(authzContext).Wait();

        // Assert - tenant can see the image
        Assert.True(authzContext.HasSucceeded);
    }

    [Test]
    public void Read_UserIsOwnerAndItemBelongsToReturnProtocol_ReturnsSuccess()
    {
        _image.ReturnProtocol = _returnProtocol;

        var requirement = ItemAuthorizationHandler.Operations.Read;

        // Mock current user
        _authenticateService.Setup(x => x.TryGetCurrentUserId()).Returns(ownerId);

        var authzContext = new AuthorizationHandlerContext(new List<IAuthorizationRequirement> { requirement },
            _owner, _image);

        // Act
        _imageAuthorizationHandler.HandleAsync(authzContext).Wait();

        // Assert - tenant can see the image
        Assert.True(authzContext.HasSucceeded);
    }

    #endregion

    // TODO: add tests for other operations
}
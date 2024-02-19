using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Moq;
using PujcovadloServer.Authentication;
using PujcovadloServer.AuthorizationHandlers.Image;
using PujcovadloServer.Business.Enums;
using PujcovadloServer.Business.Services.Interfaces;

namespace Tests.AuthorizationHandlers.Image;

public class ImageTenantAuthorizationHandlerTest
{
    ImageTenantAuthorizationHandler _authorizationHandler;
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

    private PujcovadloServer.Business.Entities.PickupProtocol _pickupProtocol;

    private PujcovadloServer.Business.Entities.ReturnProtocol _returnProtocol;

    private PujcovadloServer.Business.Entities.Image _image;


    [SetUp]
    public void Setup()
    {
        _authenticateService = new Mock<IAuthenticateService>();
        _authorizationHandler = new ImageTenantAuthorizationHandler(_authenticateService.Object);

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

        _pickupProtocol = new PujcovadloServer.Business.Entities.PickupProtocol
        {
            Description = "Description",
        };

        _returnProtocol = new PujcovadloServer.Business.Entities.ReturnProtocol
        {
            Description = "Description",
        };

        _image = new PujcovadloServer.Business.Entities.Image
        {
            Owner = new ApplicationUser { Id = _ownerId },
            Item = _item,
            Name = "TestImage.jpg",
            Extension = ".jpg",
            MimeType = "image/jpeg",
        };
    }

    #region NoTenantUsers

    [Test]
    public async Task HandleAsync_UserDoesIsNotTheTenantAndImageBelongsToItem_ReturnsFalse()
    {
        // arrange
        _image.Item = _item;
        _image.PickupProtocol = null;
        _image.ReturnProtocol = null;

        var users = new List<ClaimsPrincipal> { _unauthenticatedUser, _user, _owner, _admin };
        var requirements = new List<IAuthorizationRequirement> { ImageOperations.Read, ImageOperations.Delete };

        foreach (var user in users)
        {
            // Mock user
            _authenticateService.Setup(x => x.TryGetCurrentUserId()).Returns(user.FindFirstValue(ClaimTypes.Sid));

            // act - every requirement should fail
            foreach (var requirement in requirements)
            {
                var context = new AuthorizationHandlerContext(new List<IAuthorizationRequirement> { requirement }, user,
                    _image);
                await _authorizationHandler.HandleAsync(context);
                Assert.That(context.HasSucceeded, Is.False);
            }
        }
    }

    [Test]
    public async Task HandleAsync_UserIsNotTheTenantAndImageBelongsToPickupProtocol_ReturnsFalse()
    {
        // arrange
        _loan.Item = _item;
        _pickupProtocol.Loan = _loan;
        _image.Item = null;
        _image.PickupProtocol = _pickupProtocol;
        _image.ReturnProtocol = null;

        var users = new List<ClaimsPrincipal> { _unauthenticatedUser, _user, _owner, _admin };
        var requirements = new List<IAuthorizationRequirement> { ImageOperations.Read, ImageOperations.Delete };

        foreach (var user in users)
        {
            // Mock user
            _authenticateService.Setup(x => x.TryGetCurrentUserId()).Returns(user.FindFirstValue(ClaimTypes.Sid));

            // act - every requirement should fail
            foreach (var requirement in requirements)
            {
                var context = new AuthorizationHandlerContext(new List<IAuthorizationRequirement> { requirement }, user,
                    _image);
                await _authorizationHandler.HandleAsync(context);
                Assert.That(context.HasSucceeded, Is.False);
            }
        }
    }

    [Test]
    public async Task HandleAsync_UserIsNotTheTenantAndImageBelongsToReturnProtocol_ReturnsFalse()
    {
        // arrange
        _loan.Item = _item;
        _returnProtocol.Loan = _loan;
        _image.Item = null;
        _image.PickupProtocol = null;
        _image.ReturnProtocol = _returnProtocol;

        var users = new List<ClaimsPrincipal> { _unauthenticatedUser, _user, _owner, _admin };
        var requirements = new List<IAuthorizationRequirement> { ImageOperations.Read, ImageOperations.Delete };

        foreach (var user in users)
        {
            // Mock user
            _authenticateService.Setup(x => x.TryGetCurrentUserId()).Returns(user.FindFirstValue(ClaimTypes.Sid));

            // act - every requirement should fail
            foreach (var requirement in requirements)
            {
                var context = new AuthorizationHandlerContext(new List<IAuthorizationRequirement> { requirement }, user,
                    _image);
                await _authorizationHandler.HandleAsync(context);
                Assert.That(context.HasSucceeded, Is.False);
            }
        }
    }

    #endregion

    #region Read

    [Test]
    public async Task Read_UserIsTenantAndItemBelongsToItem_ReturnsTrue()
    {
        _image.Item = _item;
        _image.PickupProtocol = null;
        _image.ReturnProtocol = null;

        // Mock user
        _authenticateService.Setup(x => x.TryGetCurrentUserId()).Returns(_tenant.FindFirstValue(ClaimTypes.Sid));

        var context =
            new AuthorizationHandlerContext(new List<IAuthorizationRequirement> { ImageOperations.Read }, _tenant,
                _image);
        await _authorizationHandler.HandleAsync(context);

        // assert - should be true because the tenant can read the image of the pickup protocol which is related to his loan
        Assert.That(context.HasSucceeded, Is.False);
    }

    [Test]
    public async Task Read_UserIsTenantAndItemBelongsToPickupProtocol_ReturnsTrue()
    {
        _loan.Item = _item;
        _pickupProtocol.Loan = _loan;
        _image.Item = null;
        _image.PickupProtocol = _pickupProtocol;
        _image.ReturnProtocol = null;

        // Mock user
        _authenticateService.Setup(x => x.TryGetCurrentUserId()).Returns(_tenant.FindFirstValue(ClaimTypes.Sid));

        var context =
            new AuthorizationHandlerContext(new List<IAuthorizationRequirement> { ImageOperations.Read }, _tenant,
                _image);
        await _authorizationHandler.HandleAsync(context);

        // assert - should be true because the tenant can read the image of the pickup protocol which is related to his loan
        Assert.That(context.HasSucceeded, Is.True);
    }

    [Test]
    public async Task Read_UserIsTenantAndItemBelongsToReturnProtocol_ReturnsTrue()
    {
        _loan.Item = _item;
        _returnProtocol.Loan = _loan;
        _image.Item = null;
        _image.PickupProtocol = null;
        _image.ReturnProtocol = _returnProtocol;

        // Mock user
        _authenticateService.Setup(x => x.TryGetCurrentUserId()).Returns(_tenant.FindFirstValue(ClaimTypes.Sid));

        var context =
            new AuthorizationHandlerContext(new List<IAuthorizationRequirement> { ImageOperations.Read }, _tenant,
                _image);
        await _authorizationHandler.HandleAsync(context);

        // assert - should be true because the tenant can read the image of the pickup protocol which is related to his loan
        Assert.That(context.HasSucceeded, Is.True);
    }

    #endregion

    #region Delete

    [Test]
    public async Task Delete_UserIsTenantAndItemBelongsToItem_ReturnsTrue()
    {
        _image.Item = _item;
        _image.PickupProtocol = null;
        _image.ReturnProtocol = null;

        var requirements = new List<IAuthorizationRequirement> { ImageOperations.Delete, ImageOperations.Delete };

        // Mock user
        _authenticateService.Setup(x => x.TryGetCurrentUserId()).Returns(_tenant.FindFirstValue(ClaimTypes.Sid));

        // act - every requirement should fail
        foreach (var requirement in requirements)
        {
            var context =
                new AuthorizationHandlerContext(new List<IAuthorizationRequirement> { requirement }, _tenant, _image);
            await _authorizationHandler.HandleAsync(context);

            // assert - should be false because the tenant cant delete image of item which does not belong to him
            Assert.That(context.HasSucceeded, Is.False);
        }
    }

    [Test]
    public async Task Delete_UserIsTenantAndItemBelongsToPickupProtocol_ReturnsTrue()
    {
        _loan.Item = _item;
        _pickupProtocol.Loan = _loan;
        _image.Item = null;
        _image.PickupProtocol = _pickupProtocol;
        _image.ReturnProtocol = null;

        // Mock user
        _authenticateService.Setup(x => x.TryGetCurrentUserId()).Returns(_tenant.FindFirstValue(ClaimTypes.Sid));

        var context =
            new AuthorizationHandlerContext(new List<IAuthorizationRequirement> { ImageOperations.Delete }, _tenant,
                _image);
        await _authorizationHandler.HandleAsync(context);

        // assert - should be false because the tenant cant delete image of protocol which does not belong to him
        Assert.That(context.HasSucceeded, Is.False);
    }

    [Test]
    public async Task Delete_UserIsTenantAndItemBelongsToReturnProtocol_ReturnsTrue()
    {
        _loan.Item = _item;
        _returnProtocol.Loan = _loan;
        _image.Item = null;
        _image.PickupProtocol = null;
        _image.ReturnProtocol = _returnProtocol;

        // Mock user
        _authenticateService.Setup(x => x.TryGetCurrentUserId()).Returns(_tenant.FindFirstValue(ClaimTypes.Sid));

        var context =
            new AuthorizationHandlerContext(new List<IAuthorizationRequirement> { ImageOperations.Delete }, _tenant,
                _image);
        await _authorizationHandler.HandleAsync(context);

        // assert - should be false because the tenant cant delete image of protocol which does not belong to him
        Assert.That(context.HasSucceeded, Is.False);
    }

    #endregion
}
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Moq;
using PujcovadloServer.Authentication;
using PujcovadloServer.AuthorizationHandlers.Item;
using PujcovadloServer.AuthorizationHandlers.Loan;
using PujcovadloServer.Business.Enums;
using PujcovadloServer.Business.Services.Interfaces;

namespace Tests.AuthorizationHandlers.Loan;

public class LoanOwnerAuthorizationHandlerTest
{
    LoanOwnerAuthorizationHandler _authorizationHandler;
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


    [SetUp]
    public void Setup()
    {
        _authenticateService = new Mock<IAuthenticateService>();
        _authorizationHandler = new LoanOwnerAuthorizationHandler(_authenticateService.Object);

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
    }

    #region NoOwnerUsers

    [Test]
    public async Task HandleAsync_UserDoesDoesNotHaveOwnerRole_ReturnsFalse()
    {
        var users = new List<ClaimsPrincipal> { _unauthenticatedUser, _user, _tenant, _admin };
        var requirements = new List<IAuthorizationRequirement>
        {
            LoanOperations.Read, LoanOperations.Delete, LoanOperations.Create, LoanOperations.Update,
            LoanOperations.CreatePickupProtocol, LoanOperations.CreatePickupProtocol
        };

        foreach (var user in users)
        {
            // Mock user
            _authenticateService.Setup(x => x.TryGetCurrentUserId()).Returns(user.FindFirstValue(ClaimTypes.Sid));

            // act - every requirement should fail
            foreach (var requirement in requirements)
            {
                var context = new AuthorizationHandlerContext(new List<IAuthorizationRequirement> { requirement }, user,
                    _loan);
                await _authorizationHandler.HandleAsync(context);
                Assert.That(context.HasSucceeded, Is.False);
            }
        }
    }

    #endregion

    #region Read

    [Test]
    public async Task Read_UserHasOwnerRoleButIsNotTheOwner_ReturnsFalse()
    {
        _loan.Item.Owner = new ApplicationUser { Id = "20" };

        // Mock user
        _authenticateService.Setup(x => x.TryGetCurrentUserId()).Returns(_owner.FindFirstValue(ClaimTypes.Sid));

        // act
        var context =
            new AuthorizationHandlerContext(new List<IAuthorizationRequirement> { LoanOperations.Read }, _owner,
                _loan);
        await _authorizationHandler.HandleAsync(context);

        // assert - owner cannot read other owner's loan
        Assert.That(context.HasSucceeded, Is.False);
    }

    [Test]
    public async Task Read_UserHasOwnerRoleAndIsTheOwner_ReturnsTrue()
    {
        _loan.Item.Owner = new ApplicationUser { Id = _ownerId };

        // Mock user
        _authenticateService.Setup(x => x.TryGetCurrentUserId()).Returns(_owner.FindFirstValue(ClaimTypes.Sid));

        // act
        var context =
            new AuthorizationHandlerContext(new List<IAuthorizationRequirement> { LoanOperations.Read }, _owner,
                _loan);
        await _authorizationHandler.HandleAsync(context);

        // assert - owner can read his own loan
        Assert.That(context.HasSucceeded, Is.True);
    }

    #endregion

    #region Create

    [Test]
    public async Task Create_UserHasOwnerRole_ReturnsFalse()
    {
        // Mock user
        _authenticateService.Setup(x => x.TryGetCurrentUserId()).Returns(_owner.FindFirstValue(ClaimTypes.Sid));

        // act
        var context =
            new AuthorizationHandlerContext(new List<IAuthorizationRequirement> { LoanOperations.Create }, _owner,
                _loan);
        await _authorizationHandler.HandleAsync(context);

        // assert - owner should not be able to create a loan
        Assert.That(context.HasSucceeded, Is.False);
    }

    #endregion

    #region Update

    [Test]
    public async Task Update_UserHasOwnerRoleButIsNotTheOwner_ReturnsFalse()
    {
        _loan.Item.Owner = new ApplicationUser { Id = "20" };

        // Mock user
        _authenticateService.Setup(x => x.TryGetCurrentUserId()).Returns(_owner.FindFirstValue(ClaimTypes.Sid));

        // act
        var context =
            new AuthorizationHandlerContext(new List<IAuthorizationRequirement> { LoanOperations.Update }, _owner,
                _loan);
        await _authorizationHandler.HandleAsync(context);

        // assert - owner cannot updatete other owner's loan
        Assert.That(context.HasSucceeded, Is.False);
    }

    [Test]
    public async Task Update_UserHasOwnerRoleAndIsTheOwner_ReturnsTrue()
    {
        _loan.Item.Owner = new ApplicationUser { Id = _ownerId };

        // Mock user
        _authenticateService.Setup(x => x.TryGetCurrentUserId()).Returns(_owner.FindFirstValue(ClaimTypes.Sid));

        // act
        var context =
            new AuthorizationHandlerContext(new List<IAuthorizationRequirement> { LoanOperations.Update }, _owner,
                _loan);
        await _authorizationHandler.HandleAsync(context);

        // assert - owner can update his own loan
        Assert.That(context.HasSucceeded, Is.True);
    }

    #endregion

    #region Delete

    [Test]
    public async Task Delete_UserHasOwnerRoleButIsNotTheOwner_ReturnsFalse()
    {
        _loan.Item.Owner = new ApplicationUser { Id = "20" };

        // Mock user
        _authenticateService.Setup(x => x.TryGetCurrentUserId()).Returns(_owner.FindFirstValue(ClaimTypes.Sid));

        // act
        var context =
            new AuthorizationHandlerContext(new List<IAuthorizationRequirement> { LoanOperations.Delete }, _owner,
                _loan);
        await _authorizationHandler.HandleAsync(context);

        // assert - owner cannot read other owner's loan
        Assert.That(context.HasSucceeded, Is.False);
    }

    [Test]
    public async Task Delete_UserHasOwnerRoleAndIsTheOwner_ReturnsFalse()
    {
        _loan.Item.Owner = new ApplicationUser { Id = _ownerId };

        // Mock user
        _authenticateService.Setup(x => x.TryGetCurrentUserId()).Returns(_owner.FindFirstValue(ClaimTypes.Sid));

        // act
        var context =
            new AuthorizationHandlerContext(new List<IAuthorizationRequirement> { LoanOperations.Delete }, _owner,
                _loan);
        await _authorizationHandler.HandleAsync(context);

        // assert - even the owner cannot delete his loan
        Assert.That(context.HasSucceeded, Is.False);
    }

    #endregion

    #region CreatePickupProtocol

    [Test]
    public async Task CreatePickupProtocol_UserHasOwnerRoleButIsNotTheOwner_ReturnsFalse()
    {
        _loan.Item.Owner = new ApplicationUser { Id = "20" };

        // Mock user
        _authenticateService.Setup(x => x.TryGetCurrentUserId()).Returns(_owner.FindFirstValue(ClaimTypes.Sid));

        // act
        var context =
            new AuthorizationHandlerContext(new List<IAuthorizationRequirement> { LoanOperations.CreatePickupProtocol },
                _owner,
                _loan);
        await _authorizationHandler.HandleAsync(context);

        // assert - owner cannot create pickup protocol other owner's loan
        Assert.That(context.HasSucceeded, Is.False);
    }

    [Test]
    public async Task CreatePickupProtocol_UserHasOwnerRoleAndIsTheOwner_ReturnsTrue()
    {
        _loan.Item.Owner = new ApplicationUser { Id = _ownerId };

        // Mock user
        _authenticateService.Setup(x => x.TryGetCurrentUserId()).Returns(_owner.FindFirstValue(ClaimTypes.Sid));

        // act
        var context =
            new AuthorizationHandlerContext(new List<IAuthorizationRequirement> { LoanOperations.CreatePickupProtocol },
                _owner,
                _loan);
        await _authorizationHandler.HandleAsync(context);

        // assert - owner can create protocol for his own loan
        Assert.That(context.HasSucceeded, Is.True);
    }

    #endregion

    #region CreateReturnProtocol

    [Test]
    public async Task CreateReturnProtocol_UserHasOwnerRoleButIsNotTheOwner_ReturnsFalse()
    {
        _loan.Item.Owner = new ApplicationUser { Id = "20" };

        // Mock user
        _authenticateService.Setup(x => x.TryGetCurrentUserId()).Returns(_owner.FindFirstValue(ClaimTypes.Sid));

        // act
        var context =
            new AuthorizationHandlerContext(new List<IAuthorizationRequirement> { LoanOperations.CreateReturnProtocol },
                _owner,
                _loan);
        await _authorizationHandler.HandleAsync(context);

        // assert - owner cannot read other owner's loan
        Assert.That(context.HasSucceeded, Is.False);
    }

    [Test]
    public async Task CreateReturnProtocol_UserHasOwnerRoleAndIsTheOwner_ReturnsTrue()
    {
        _loan.Item.Owner = new ApplicationUser { Id = _ownerId };

        // Mock user
        _authenticateService.Setup(x => x.TryGetCurrentUserId()).Returns(_owner.FindFirstValue(ClaimTypes.Sid));

        // act
        var context =
            new AuthorizationHandlerContext(new List<IAuthorizationRequirement> { LoanOperations.CreateReturnProtocol },
                _owner,
                _loan);
        await _authorizationHandler.HandleAsync(context);

        // assert - owner can read his own loan
        Assert.That(context.HasSucceeded, Is.True);
    }

    #endregion
}
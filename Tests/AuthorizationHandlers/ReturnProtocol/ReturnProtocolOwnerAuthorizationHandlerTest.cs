using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Moq;
using PujcovadloServer.Authentication;
using PujcovadloServer.AuthorizationHandlers.ReturnProtocol;
using PujcovadloServer.Business.Enums;
using PujcovadloServer.Business.Services.Interfaces;

namespace Tests.AuthorizationHandlers.ReturnProtocol;

public class ReturnProtocolOwnerAuthorizationHandlerTest
{
    ReturnProtocolOwnerAuthorizationHandler _authorizationHandler;
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

    private PujcovadloServer.Business.Entities.ReturnProtocol _returnProtocol;


    [SetUp]
    public void Setup()
    {
        _authenticateService = new Mock<IAuthenticateService>();
        _authorizationHandler = new ReturnProtocolOwnerAuthorizationHandler(_authenticateService.Object);

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

        _returnProtocol = new PujcovadloServer.Business.Entities.ReturnProtocol
        {
            Description = "Description",
            Loan = _loan,
        };
    }

    #region NoOwnerUsers

    [Test]
    public async Task HandleAsync_UserDoesNotHaveAdminRole_ReturnsFalse()
    {
        var users = new List<ClaimsPrincipal> { _unauthenticatedUser, _user, _tenant, _admin };
        var requirements = new List<IAuthorizationRequirement>
        {
            ReturnProtocolOperations.Read, ReturnProtocolOperations.Delete, ReturnProtocolOperations.Update
        };

        foreach (var user in users)
        {
            // Mock user
            _authenticateService.Setup(x => x.TryGetCurrentUserId()).Returns(user.FindFirstValue(ClaimTypes.Sid));

            // act - every requirement should fail
            foreach (var requirement in requirements)
            {
                var context = new AuthorizationHandlerContext(new List<IAuthorizationRequirement> { requirement }, user,
                    _returnProtocol);
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
        _returnProtocol.Loan.Item.Owner = new ApplicationUser { Id = "20" };

        // Mock user
        _authenticateService.Setup(x => x.TryGetCurrentUserId()).Returns(_owner.FindFirstValue(ClaimTypes.Sid));

        // act
        var context =
            new AuthorizationHandlerContext(new List<IAuthorizationRequirement> { ReturnProtocolOperations.Read },
                _owner,
                _returnProtocol);
        await _authorizationHandler.HandleAsync(context);

        // assert - owner cannot read other users protocol
        Assert.That(context.HasSucceeded, Is.False);
    }

    [Test]
    public async Task Read_UserHasOwnerRoleAndIsTheOwner_ReturnsTrue()
    {
        _returnProtocol.Loan.Item.Owner = new ApplicationUser { Id = _ownerId };

        // Mock user
        _authenticateService.Setup(x => x.TryGetCurrentUserId()).Returns(_owner.FindFirstValue(ClaimTypes.Sid));

        // act
        var context =
            new AuthorizationHandlerContext(new List<IAuthorizationRequirement> { ReturnProtocolOperations.Read },
                _owner,
                _returnProtocol);
        await _authorizationHandler.HandleAsync(context);

        // assert - owner can read his own protocol
        Assert.That(context.HasSucceeded, Is.True);
    }

    #endregion

    #region Update

    [Test]
    public async Task Update_UserHasOwnerRoleButIsNotTheOwner_ReturnsFalse()
    {
        _returnProtocol.Loan.Item.Owner = new ApplicationUser { Id = "20" };

        // Mock user
        _authenticateService.Setup(x => x.TryGetCurrentUserId()).Returns(_owner.FindFirstValue(ClaimTypes.Sid));

        // act
        var context =
            new AuthorizationHandlerContext(new List<IAuthorizationRequirement> { ReturnProtocolOperations.Update },
                _owner,
                _returnProtocol);
        await _authorizationHandler.HandleAsync(context);

        // assert - owner cannot update other users protocol
        Assert.That(context.HasSucceeded, Is.False);
    }

    [Test]
    public async Task Update_UserHasOwnerRoleAndIsTheOwner_ReturnsTrue()
    {
        _returnProtocol.Loan.Item.Owner = new ApplicationUser { Id = _ownerId };

        // Mock user
        _authenticateService.Setup(x => x.TryGetCurrentUserId()).Returns(_owner.FindFirstValue(ClaimTypes.Sid));

        // act
        var context =
            new AuthorizationHandlerContext(new List<IAuthorizationRequirement> { ReturnProtocolOperations.Update },
                _owner,
                _returnProtocol);
        await _authorizationHandler.HandleAsync(context);

        // assert - owner can update his own protocol
        Assert.That(context.HasSucceeded, Is.True);
    }

    #endregion

    #region Delete

    [Test]
    public async Task Delete_UserHasOwnerRoleButIsNotTheOwner_ReturnsFalse()
    {
        _returnProtocol.Loan.Item.Owner = new ApplicationUser { Id = "20" };

        // Mock user
        _authenticateService.Setup(x => x.TryGetCurrentUserId()).Returns(_owner.FindFirstValue(ClaimTypes.Sid));

        // act
        var context =
            new AuthorizationHandlerContext(new List<IAuthorizationRequirement> { ReturnProtocolOperations.Delete },
                _owner,
                _returnProtocol);
        await _authorizationHandler.HandleAsync(context);

        // assert - owner cannot delete other users protocol
        Assert.That(context.HasSucceeded, Is.False);
    }

    [Test]
    public async Task Delete_UserHasOwnerRoleAndIsTheOwner_ReturnsFalse()
    {
        _returnProtocol.Loan.Item.Owner = new ApplicationUser { Id = _ownerId };

        // Mock user
        _authenticateService.Setup(x => x.TryGetCurrentUserId()).Returns(_owner.FindFirstValue(ClaimTypes.Sid));

        // act
        var context =
            new AuthorizationHandlerContext(new List<IAuthorizationRequirement> { ReturnProtocolOperations.Delete },
                _owner,
                _returnProtocol);
        await _authorizationHandler.HandleAsync(context);

        // assert - even the owner cannot delete his own protocol
        Assert.That(context.HasSucceeded, Is.False);
    }

    #endregion
}
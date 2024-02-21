using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Moq;
using PujcovadloServer.Authentication;
using PujcovadloServer.AuthorizationHandlers.Item;
using PujcovadloServer.AuthorizationHandlers.Loan;
using PujcovadloServer.Business.Enums;
using PujcovadloServer.Business.Services.Interfaces;

namespace Tests.AuthorizationHandlers.Loan;

public class LoanTenantAuthorizationHandlerTest
{
    LoanTenantAuthorizationHandler _authorizationHandler;
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
        _authorizationHandler = new LoanTenantAuthorizationHandler(_authenticateService.Object);

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

    #region NoTenantUsers

    [Test]
    public async Task HandleAsync_UserDoesDoesNotHaveTenantRole_ReturnsFalse()
    {
        var users = new List<ClaimsPrincipal> { _unauthenticatedUser, _user, _owner, _admin };
        var requirements = new List<IAuthorizationRequirement>
        {
            LoanOperations.Read, LoanOperations.Delete, LoanOperations.Create, LoanOperations.Update,
            LoanOperations.CreatePickupProtocol, LoanOperations.CreatePickupProtocol, LoanOperations.CreateReview
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
    public async Task Read_UserHasTenantRoleButIsNotTheTenant_ReturnsFalse()
    {
        _loan.Tenant = new ApplicationUser { Id = "20" };

        // Mock user
        _authenticateService.Setup(x => x.TryGetCurrentUserId()).Returns(_tenant.FindFirstValue(ClaimTypes.Sid));

        // act
        var context =
            new AuthorizationHandlerContext(new List<IAuthorizationRequirement> { LoanOperations.Read }, _tenant,
                _loan);
        await _authorizationHandler.HandleAsync(context);

        // assert - tenant cannot read other tenant's loan
        Assert.That(context.HasSucceeded, Is.False);
    }

    [Test]
    public async Task Read_UserHasTenantRoleAndIsTheTenant_ReturnsTrue()
    {
        _loan.Tenant = new ApplicationUser { Id = _tenantId };

        // Mock user
        _authenticateService.Setup(x => x.TryGetCurrentUserId()).Returns(_tenant.FindFirstValue(ClaimTypes.Sid));

        // act
        var context =
            new AuthorizationHandlerContext(new List<IAuthorizationRequirement> { LoanOperations.Read }, _tenant,
                _loan);
        await _authorizationHandler.HandleAsync(context);

        // assert - tenant can read his own loan
        Assert.That(context.HasSucceeded, Is.True);
    }

    #endregion

    #region Create

    [Test]
    public async Task Create_UserHasTenantRole_ReturnsFalse()
    {
        // Mock user
        _authenticateService.Setup(x => x.TryGetCurrentUserId()).Returns(_tenant.FindFirstValue(ClaimTypes.Sid));

        // act
        var context =
            new AuthorizationHandlerContext(new List<IAuthorizationRequirement> { LoanOperations.Create }, _tenant,
                _loan);
        await _authorizationHandler.HandleAsync(context);

        // assert - tenant should be able to create new loan
        Assert.That(context.HasSucceeded, Is.True);
    }

    #endregion

    #region Update

    [Test]
    public async Task Update_UserHasTenantRoleButIsNotTheTenant_ReturnsFalse()
    {
        _loan.Tenant = new ApplicationUser { Id = "20" };

        // Mock user
        _authenticateService.Setup(x => x.TryGetCurrentUserId()).Returns(_tenant.FindFirstValue(ClaimTypes.Sid));

        // act
        var context =
            new AuthorizationHandlerContext(new List<IAuthorizationRequirement> { LoanOperations.Update }, _tenant,
                _loan);
        await _authorizationHandler.HandleAsync(context);

        // assert - tenant cannot updatete other tenant's loan
        Assert.That(context.HasSucceeded, Is.False);
    }

    [Test]
    public async Task Update_UserHasTenantRoleAndIsTheTenant_ReturnsTrue()
    {
        _loan.Tenant = new ApplicationUser { Id = _tenantId };

        // Mock user
        _authenticateService.Setup(x => x.TryGetCurrentUserId()).Returns(_tenant.FindFirstValue(ClaimTypes.Sid));

        // act
        var context =
            new AuthorizationHandlerContext(new List<IAuthorizationRequirement> { LoanOperations.Update }, _tenant,
                _loan);
        await _authorizationHandler.HandleAsync(context);

        // assert - tenant can update his own loan
        Assert.That(context.HasSucceeded, Is.True);
    }

    #endregion

    #region Delete

    [Test]
    public async Task Delete_UserHasTenantRoleButIsNotTheTenant_ReturnsFalse()
    {
        _loan.Tenant = new ApplicationUser { Id = "20" };

        // Mock user
        _authenticateService.Setup(x => x.TryGetCurrentUserId()).Returns(_tenant.FindFirstValue(ClaimTypes.Sid));

        // act
        var context =
            new AuthorizationHandlerContext(new List<IAuthorizationRequirement> { LoanOperations.Delete }, _tenant,
                _loan);
        await _authorizationHandler.HandleAsync(context);

        // assert - tenant cannot delete other tenant's loan
        Assert.That(context.HasSucceeded, Is.False);
    }

    [Test]
    public async Task Delete_UserHasTenantRoleAndIsTheTenant_ReturnsFalse()
    {
        _loan.Tenant = new ApplicationUser { Id = _tenantId };

        // Mock user
        _authenticateService.Setup(x => x.TryGetCurrentUserId()).Returns(_tenant.FindFirstValue(ClaimTypes.Sid));

        // act
        var context =
            new AuthorizationHandlerContext(new List<IAuthorizationRequirement> { LoanOperations.Delete }, _tenant,
                _loan);
        await _authorizationHandler.HandleAsync(context);

        // assert - even the tenant cannot delete his own loan
        Assert.That(context.HasSucceeded, Is.False);
    }

    #endregion

    #region CreatePickupProtocol

    [Test]
    public async Task CreatePickupProtocol_UserHasTenantRoleButIsNotTheTenant_ReturnsFalse()
    {
        _loan.Tenant = new ApplicationUser { Id = "20" };

        // Mock user
        _authenticateService.Setup(x => x.TryGetCurrentUserId()).Returns(_tenant.FindFirstValue(ClaimTypes.Sid));

        // act
        var context =
            new AuthorizationHandlerContext(new List<IAuthorizationRequirement> { LoanOperations.CreatePickupProtocol },
                _tenant,
                _loan);
        await _authorizationHandler.HandleAsync(context);

        // assert - tenant cannot create protocol
        Assert.That(context.HasSucceeded, Is.False);
    }

    [Test]
    public async Task CreatePickupProtocol_UserHasTenantRoleAndIsTheTenant_ReturnsTrue()
    {
        _loan.Tenant = new ApplicationUser { Id = _tenantId };

        // Mock user
        _authenticateService.Setup(x => x.TryGetCurrentUserId()).Returns(_tenant.FindFirstValue(ClaimTypes.Sid));

        // act
        var context =
            new AuthorizationHandlerContext(new List<IAuthorizationRequirement> { LoanOperations.CreatePickupProtocol },
                _tenant,
                _loan);
        await _authorizationHandler.HandleAsync(context);

        // assert - tenant cannot create protocol
        Assert.That(context.HasSucceeded, Is.False);
    }

    #endregion

    #region CreateReturnProtocol

    [Test]
    public async Task CreateReturnProtocol_UserHasTenantRoleButIsNotTheTenant_ReturnsFalse()
    {
        _loan.Tenant = new ApplicationUser { Id = "20" };

        // Mock user
        _authenticateService.Setup(x => x.TryGetCurrentUserId()).Returns(_tenant.FindFirstValue(ClaimTypes.Sid));

        // act
        var context =
            new AuthorizationHandlerContext(new List<IAuthorizationRequirement> { LoanOperations.CreateReturnProtocol },
                _tenant,
                _loan);
        await _authorizationHandler.HandleAsync(context);

        // assert - tenant cannot create protocol
        Assert.That(context.HasSucceeded, Is.False);
    }

    [Test]
    public async Task CreateReturnProtocol_UserHasTenantRoleAndIsTheTenant_ReturnsTrue()
    {
        _loan.Tenant = new ApplicationUser { Id = _tenantId };

        // Mock user
        _authenticateService.Setup(x => x.TryGetCurrentUserId()).Returns(_tenant.FindFirstValue(ClaimTypes.Sid));

        // act
        var context =
            new AuthorizationHandlerContext(new List<IAuthorizationRequirement> { LoanOperations.CreateReturnProtocol },
                _tenant,
                _loan);
        await _authorizationHandler.HandleAsync(context);

        // assert - tenant cannot create protocol
        Assert.That(context.HasSucceeded, Is.False);
    }

    #endregion

    #region CreateReview

    [Test]
    public async Task CreateReview_UserHasTenantRoleButIsNotTheTenant_ReturnsFalse()
    {
        _loan.Tenant = new ApplicationUser { Id = "20" };

        // Mock user
        _authenticateService.Setup(x => x.TryGetCurrentUserId()).Returns(_tenant.FindFirstValue(ClaimTypes.Sid));

        // act
        var context =
            new AuthorizationHandlerContext(new List<IAuthorizationRequirement> { LoanOperations.CreateReview },
                _tenant, _loan);
        await _authorizationHandler.HandleAsync(context);

        // assert - tenant cannot review other tenant's loan
        Assert.That(context.HasSucceeded, Is.False);
    }

    [Test]
    public async Task CreateReview_UserHasTenantRoleAndIsTheTenant_ReturnsTrue()
    {
        _loan.Tenant = new ApplicationUser { Id = _tenantId };

        // Mock user
        _authenticateService.Setup(x => x.TryGetCurrentUserId()).Returns(_tenant.FindFirstValue(ClaimTypes.Sid));

        // act
        var context =
            new AuthorizationHandlerContext(new List<IAuthorizationRequirement> { LoanOperations.CreateReview },
                _tenant, _loan);
        await _authorizationHandler.HandleAsync(context);

        // assert - tenant can review his own loan
        Assert.That(context.HasSucceeded, Is.True);
    }

    #endregion
}
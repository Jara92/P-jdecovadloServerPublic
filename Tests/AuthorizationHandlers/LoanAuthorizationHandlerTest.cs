using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Moq;
using PujcovadloServer.Authentication;
using PujcovadloServer.AuthorizationHandlers;
using PujcovadloServer.Business.Entities;
using PujcovadloServer.Business.Enums;
using PujcovadloServer.Business.Services.Interfaces;

namespace Tests.AuthorizationHandlers;

public class LoanAuthorizationHandlerTest
{
    LoanAuthorizationHandler _loanAuthorizationHandler;
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
    /// Authenticated user with the Tenant role.
    /// </summary>
    ClaimsPrincipal _tenant;

    /// <summary>
    /// Auhenticated user with the Admin role.
    /// </summary>
    ClaimsPrincipal _admin;

    [SetUp]
    public void Setup()
    {
        _authenticateService = new Mock<IAuthenticateService>();
        _loanAuthorizationHandler = new LoanAuthorizationHandler(_authenticateService.Object);

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
        _tenant = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
        {
            new Claim(ClaimTypes.Name, "bart.simpson"), new Claim(ClaimTypes.Sid, "3"),
            new Claim(ClaimTypes.Role, UserRoles.Tenant), new Claim(ClaimTypes.Role, UserRoles.User)
        }));
        _admin = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
        {
            new Claim(ClaimTypes.Name, "homer.simpson"), new Claim(ClaimTypes.Sid, "4"),
            new Claim(ClaimTypes.Role, UserRoles.Admin)
        }));
    }

    #region ReadTests

    [Test]
    public async Task Read_WhenUserIsUnauthenticated_ShouldFail()
    {
        // Arrange
        var loan = new Loan { Id = 1 };
        var requirement = LoanAuthorizationHandler.Operations.Read;

        // user is not authenticated
        _authenticateService.Setup(a => a.TryGetCurrentUserId()).Returns((string)null);

        var authzContext = new AuthorizationHandlerContext(new List<IAuthorizationRequirement> { requirement },
            _unauthenticatedUser, loan);

        // Act
        await _loanAuthorizationHandler.HandleAsync(authzContext);

        // Assert
        Assert.False(authzContext.HasSucceeded);
    }

    [Test]
    public async Task Read_WhenUserIsAuthenticatedButNotTenantOrOwner_ShouldFail()
    {
        // Arrange
        var loan = new Loan
        {
            Id = 1, Tenant = new ApplicationUser { Id = "10" },
            Item = new Item { Owner = new ApplicationUser { Id = "11" } }
        };
        var requirement = LoanAuthorizationHandler.Operations.Read;

        // user is authenticated
        _authenticateService.Setup(a => a.TryGetCurrentUserId()).Returns("1");

        var authzContext = new AuthorizationHandlerContext(new List<IAuthorizationRequirement> { requirement },
            _user, loan);

        // Act
        await _loanAuthorizationHandler.HandleAsync(authzContext);

        // Assert
        Assert.False(authzContext.HasSucceeded);
    }

    [Test]
    public async Task Read_WhenUserIsAuthenticatedAndIsTenant_ShouldSucceed()
    {
        // Arrange
        var loan = new Loan
        {
            Id = 1, Tenant = new ApplicationUser { Id = "1" },
            Item = new Item { Owner = new ApplicationUser { Id = "11" } }
        };
        var requirement = LoanAuthorizationHandler.Operations.Read;

        // user is authenticated
        _authenticateService.Setup(a => a.TryGetCurrentUserId()).Returns("1");

        var authzContext = new AuthorizationHandlerContext(new List<IAuthorizationRequirement> { requirement },
            _user, loan);

        // Act
        await _loanAuthorizationHandler.HandleAsync(authzContext);

        // Assert
        Assert.True(authzContext.HasSucceeded);
    }

    [Test]
    public async Task Read_WhenUserIsAuthenticatedAndIsOwner_ShouldSucceed()
    {
        // Arrange
        var loan = new Loan
        {
            Id = 1, Tenant = new ApplicationUser { Id = "10" },
            Item = new Item { Owner = new ApplicationUser { Id = "1" } }
        };
        var requirement = LoanAuthorizationHandler.Operations.Read;

        // user is authenticated
        _authenticateService.Setup(a => a.TryGetCurrentUserId()).Returns("1");

        var authzContext = new AuthorizationHandlerContext(new List<IAuthorizationRequirement> { requirement },
            _user, loan);

        // Act
        await _loanAuthorizationHandler.HandleAsync(authzContext);

        // Assert
        Assert.True(authzContext.HasSucceeded);
    }

    #endregion

    #region CreateTests

    [Test]
    public void Create_WhenUserIsUnauthenticated_ShouldFail()
    {
        // Arrange
        var loan = new Loan { Id = 1 };
        var requirement = LoanAuthorizationHandler.Operations.Create;

        // user is not authenticated
        _authenticateService.Setup(a => a.TryGetCurrentUserId()).Returns((string)null);

        var authzContext = new AuthorizationHandlerContext(new List<IAuthorizationRequirement> { requirement },
            _unauthenticatedUser, loan);

        // Act
        _loanAuthorizationHandler.HandleAsync(authzContext).Wait();

        // Assert
        Assert.False(authzContext.HasSucceeded);
    }

    [Test]
    public void Create_WhenUserIsAuthenticatedButNotTenant_ShouldFail()
    {
        // Arrange
        var loan = new Loan { Id = 1 };
        var requirement = LoanAuthorizationHandler.Operations.Create;

        // user is authenticated
        _authenticateService.Setup(a => a.TryGetCurrentUserId()).Returns("1");

        var authzContext = new AuthorizationHandlerContext(new List<IAuthorizationRequirement> { requirement },
            _user, loan);

        // Act
        _loanAuthorizationHandler.HandleAsync(authzContext).Wait();

        // Assert
        Assert.False(authzContext.HasSucceeded);
    }

    [Test]
    public void Create_WhenUserIsAuthenticatedAndIsTenant_ShouldSucceed()
    {
        // Arrange
        var loan = new Loan { Id = 1 };
        var requirement = LoanAuthorizationHandler.Operations.Create;

        // user is authenticated
        _authenticateService.Setup(a => a.TryGetCurrentUserId()).Returns("3");

        var authzContext = new AuthorizationHandlerContext(new List<IAuthorizationRequirement> { requirement },
            _tenant, loan);

        // Act
        _loanAuthorizationHandler.HandleAsync(authzContext).Wait();

        // Assert
        Assert.True(authzContext.HasSucceeded);
    }

    #endregion

    #region UpdateTests

    [Test]
    public async Task Update_WhenUserIsUnauthenticated_ShouldFail()
    {
        // Arrange
        var loan = new Loan { Id = 1 };
        var requirement = LoanAuthorizationHandler.Operations.Update;

        // user is not authenticated
        _authenticateService.Setup(a => a.TryGetCurrentUserId()).Returns((string)null);

        var authzContext = new AuthorizationHandlerContext(new List<IAuthorizationRequirement> { requirement },
            _unauthenticatedUser, loan);

        // Act
        await _loanAuthorizationHandler.HandleAsync(authzContext);

        // Assert
        Assert.False(authzContext.HasSucceeded);
    }

    [Test]
    public async Task Update_WhenUserIsAuthenticatedButNotTenantOrOwner_ShouldFail()
    {
        // Arrange
        var loan = new Loan
        {
            Id = 1, Tenant = new ApplicationUser { Id = "10" },
            Item = new Item { Owner = new ApplicationUser { Id = "11" } }
        };
        var requirement = LoanAuthorizationHandler.Operations.Update;

        // user is authenticated
        _authenticateService.Setup(a => a.TryGetCurrentUserId()).Returns("1");

        var authzContext = new AuthorizationHandlerContext(new List<IAuthorizationRequirement> { requirement },
            _user, loan);

        // Act
        await _loanAuthorizationHandler.HandleAsync(authzContext);

        // Assert
        Assert.False(authzContext.HasSucceeded);
    }

    [Test]
    public async Task Update_WhenUserIsAuthenticatedAndIsTenant_ShouldSucceed()
    {
        // Arrange
        var loan = new Loan
        {
            Id = 1, Tenant = new ApplicationUser { Id = "3" },
            Item = new Item { Owner = new ApplicationUser { Id = "11" } }
        };
        var requirement = LoanAuthorizationHandler.Operations.Update;

        // user is authenticated
        _authenticateService.Setup(a => a.TryGetCurrentUserId()).Returns("3");

        var authzContext = new AuthorizationHandlerContext(new List<IAuthorizationRequirement> { requirement },
            _tenant, loan);

        // Act
        await _loanAuthorizationHandler.HandleAsync(authzContext);

        // Assert
        Assert.True(authzContext.HasSucceeded);
    }

    [Test]
    public async Task Update_WhenUserIsAuthenticatedAndIsOwner_ShouldSucceed()
    {
        // Arrange
        var loan = new Loan
        {
            Id = 1, Tenant = new ApplicationUser { Id = "10" },
            Item = new Item { Owner = new ApplicationUser { Id = "2" } }
        };
        var requirement = LoanAuthorizationHandler.Operations.Update;

        // user is authenticated
        _authenticateService.Setup(a => a.TryGetCurrentUserId()).Returns("2");

        var authzContext = new AuthorizationHandlerContext(new List<IAuthorizationRequirement> { requirement },
            _owner, loan);

        // Act
        await _loanAuthorizationHandler.HandleAsync(authzContext);

        // Assert
        Assert.True(authzContext.HasSucceeded);
    }

    #endregion

    #region DeleteTests

    [Test]
    public void Delete_WhenUserIsUnauthenticated_ShouldFail()
    {
        // Arrange
        var loan = new Loan { Id = 1 };
        var requirement = LoanAuthorizationHandler.Operations.Delete;

        // user is not authenticated
        _authenticateService.Setup(a => a.TryGetCurrentUserId()).Returns((string)null);

        var authzContext = new AuthorizationHandlerContext(new List<IAuthorizationRequirement> { requirement },
            _unauthenticatedUser, loan);

        // Act
        _loanAuthorizationHandler.HandleAsync(authzContext).Wait();

        // Assert
        Assert.False(authzContext.HasSucceeded);
    }

    [Test]
    public void Delete_WhenUserIsAuthenticated_ShouldFail()
    {
        // Arrange
        var loan = new Loan { Id = 1 };
        var requirement = LoanAuthorizationHandler.Operations.Delete;

        // user is authenticated
        _authenticateService.Setup(a => a.TryGetCurrentUserId()).Returns("1");

        var authzContext = new AuthorizationHandlerContext(new List<IAuthorizationRequirement> { requirement },
            _user, loan);

        // Act
        _loanAuthorizationHandler.HandleAsync(authzContext).Wait();

        // Assert
        Assert.False(authzContext.HasSucceeded);
    }

    [Test]
    public void Delete_WhenUserIsTenant_ShouldFail()
    {
        // Arrange
        var loan = new Loan
        {
            Id = 1, Tenant = new ApplicationUser { Id = "3" },
            Item = new Item { Owner = new ApplicationUser { Id = "11" } }
        };
        var requirement = LoanAuthorizationHandler.Operations.Delete;

        // user is authenticated
        _authenticateService.Setup(a => a.TryGetCurrentUserId()).Returns("3");

        var authzContext = new AuthorizationHandlerContext(new List<IAuthorizationRequirement> { requirement },
            _tenant, loan);

        // Act
        _loanAuthorizationHandler.HandleAsync(authzContext).Wait();

        // Assert
        Assert.False(authzContext.HasSucceeded);
    }

    [Test]
    public void Delete_WhenUserIsOwner_ShouldFail()
    {
        // Arrange
        var loan = new Loan
        {
            Id = 1, Tenant = new ApplicationUser { Id = "10" },
            Item = new Item { Owner = new ApplicationUser { Id = "2" } }
        };
        var requirement = LoanAuthorizationHandler.Operations.Delete;

        // user is authenticated
        _authenticateService.Setup(a => a.TryGetCurrentUserId()).Returns("2");

        var authzContext = new AuthorizationHandlerContext(new List<IAuthorizationRequirement> { requirement },
            _owner, loan);

        // Act
        _loanAuthorizationHandler.HandleAsync(authzContext).Wait();

        // Assert
        Assert.False(authzContext.HasSucceeded);
    }

    #endregion

    #region CreatePickupProtocolTests

    [Test]
    public void CreatePickupProtocol_WhenUserIsUnauthenticated_ShouldFail()
    {
        // Arrange
        var loan = new Loan { Id = 1 };
        var requirement = LoanAuthorizationHandler.Operations.CreatePickupProtocol;

        // user is not authenticated
        _authenticateService.Setup(a => a.TryGetCurrentUserId()).Returns((string)null);

        var authzContext = new AuthorizationHandlerContext(new List<IAuthorizationRequirement> { requirement },
            _unauthenticatedUser, loan);

        // Act
        _loanAuthorizationHandler.HandleAsync(authzContext).Wait();

        // Assert
        Assert.False(authzContext.HasSucceeded);
    }

    [Test]
    public void CreatePickupProtocol_WhenUserIsAuthenticatedButNotOwner_ShouldFail()
    {
        // Arrange
        var loan = new Loan
        {
            Id = 1, Tenant = new ApplicationUser { Id = "10" },
            Item = new Item { Owner = new ApplicationUser { Id = "11" } }
        };
        var requirement = LoanAuthorizationHandler.Operations.CreatePickupProtocol;

        // user is authenticated
        _authenticateService.Setup(a => a.TryGetCurrentUserId()).Returns("1");

        var authzContext = new AuthorizationHandlerContext(new List<IAuthorizationRequirement> { requirement },
            _user, loan);

        // Act
        _loanAuthorizationHandler.HandleAsync(authzContext).Wait();

        // Assert
        Assert.False(authzContext.HasSucceeded);
    }

    [Test]
    public void CreatePickupProtocol_WhenUserIsAuthenticatedAndIsTenant_ShouldFail()
    {
        // Arrange
        var loan = new Loan
        {
            Id = 1, Tenant = new ApplicationUser { Id = "3" },
            Item = new Item { Owner = new ApplicationUser { Id = "11" } }
        };
        var requirement = LoanAuthorizationHandler.Operations.CreatePickupProtocol;

        // user is authenticated
        _authenticateService.Setup(a => a.TryGetCurrentUserId()).Returns("3");

        var authzContext = new AuthorizationHandlerContext(new List<IAuthorizationRequirement> { requirement },
            _tenant, loan);

        // Act
        _loanAuthorizationHandler.HandleAsync(authzContext).Wait();

        // Assert - Tenant can't create the protocol!
        Assert.False(authzContext.HasSucceeded);
    }

    [Test]
    public void CreatePickupProtocol_WhenUserIsAuthenticatedAndIsOwner_ShouldSucceed()
    {
        // Arrange
        var loan = new Loan
        {
            Id = 1, Tenant = new ApplicationUser { Id = "10" },
            Item = new Item { Owner = new ApplicationUser { Id = "2" } }
        };
        var requirement = LoanAuthorizationHandler.Operations.CreatePickupProtocol;

        // user is authenticated
        _authenticateService.Setup(a => a.TryGetCurrentUserId()).Returns("2");

        var authzContext = new AuthorizationHandlerContext(new List<IAuthorizationRequirement> { requirement },
            _owner, loan);

        // Act
        _loanAuthorizationHandler.HandleAsync(authzContext).Wait();

        // Assert - the owner is the only one who can create the protocol
        Assert.True(authzContext.HasSucceeded);
    }

    #endregion
}
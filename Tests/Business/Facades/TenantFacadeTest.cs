

using AutoMapper;
using PujcovadloServer.Business.Factories.State;
using PujcovadloServer.Business.Interfaces;
using PujcovadloServer.Business.Services;
using PujcovadloServer.Data.Repositories;
using Moq;
using PujcovadloServer.Authentication;
using PujcovadloServer.Authentication.Exceptions;
using PujcovadloServer.Business.Entities;
using PujcovadloServer.Business.Exceptions;
using PujcovadloServer.Business.Filters;
using PujcovadloServer.Business.Services.Interfaces;
using PujcovadloServer.Lib;

namespace Tests.Business.Facades;

public class TenantFacadeTest
{
    
    private TenantFacade _tenantFasade;
    private Mock<LoanService> _loanService;
    private Mock<ItemService> _itemService;
    private Mock<IAuthenticateService> _authenticateService;
    private Mock<IMapper> _mapper;
    
    private ApplicationUser _user;
    
    [SetUp]
    public void Setup()
    {
        _loanService = new Mock<LoanService>(null, null);
        _itemService= new Mock<ItemService>(null);
        _authenticateService = new Mock<IAuthenticateService>();
        _mapper = new Mock<IMapper>();
        
        _tenantFasade = new TenantFacade(_loanService.Object, _itemService.Object, _authenticateService.Object, _mapper.Object);
        
        _user =  new ApplicationUser(){Id="1"};
    }

    #region GetMyLoans
    [Test]
    public void GetMyLoans_UnauthenticatedUser_ThrowsException()
    {
        // Authentication service will throw NotAuthenticatedException
        _authenticateService.Setup(o => o.GetCurrentUser()).Throws<NotAuthenticatedException>();
        
        // Must throw NotAuthenticatedException because no user is authenticated
        Assert.ThrowsAsync<NotAuthenticatedException>(async () => await _tenantFasade.GetMyLoans(new LoanFilter()));
        
        // Verify that GetCurrentUser was called
        _authenticateService.Verify(o => o.GetCurrentUser(), Times.Once);
    }
    
    [Test]
    public async Task GetMyLoans_AuthenticatedUser_ReturnsLoans()
    {
        var loanFilter = new LoanFilter() { Page = 1, PageSize = 10};
        var loans = new List<Loan>()
        {
            new Loan{Id = 1, Tenant = _user},
            new Loan{Id = 2, Tenant = _user},
            new Loan{Id = 3, Tenant = _user}
        };
        var resultLoans = new PaginatedList<Loan>(loans, loans.Count, loanFilter.Page, loanFilter.PageSize);
        
        // Arrange
        _authenticateService.Setup(o => o.GetCurrentUser()).ReturnsAsync(_user);
        _loanService.Setup(o => o.GetLoansByTenant(_user, loanFilter)).ReturnsAsync(resultLoans);
        
        // Act
        var result = await _tenantFasade.GetMyLoans(loanFilter);
        
        // verfiy that GetCurrentUser was called
        _authenticateService.Verify(o => o.GetCurrentUser(), Times.Once);
        
        // verify that GetLoansByTenant was called
        _loanService.Verify(o => o.GetLoansByTenant(_user, loanFilter), Times.Once);

        // must be the same
        Assert.That(result, Is.EqualTo(resultLoans));
    }
    #endregion
    
    #region GetMyLoan
    [Test]
    public void GetMyLoan_UnauthenticatedUser_ThrowsException()
    {
        // Authentication service will throw NotAuthenticatedException
        _authenticateService.Setup(o => o.GetCurrentUser()).Throws<NotAuthenticatedException>();
        
        // Must throw NotAuthenticatedException because no user is authenticated
        Assert.ThrowsAsync<NotAuthenticatedException>(async () => await _tenantFasade.GetMyLoan(1));
        
        // Verify that GetCurrentUser was called
        _authenticateService.Verify(o => o.GetCurrentUser(), Times.Once);
    }
    
    [Test]
    public void GetMyLoan_LoanNotFound_ThrowsException()
    {
        // Arrange
        var loanId = 1;
        var loan = new Loan{Id = loanId, Tenant = _user};
        
        // Authentication service will return the user
        _authenticateService.Setup(o => o.GetCurrentUser()).ReturnsAsync(_user);
        
        // Loan service will return null
        _loanService.Setup(o => o.Get(loanId, true)).ReturnsAsync((Loan)null);
        
        // Must throw EntityNotFoundException because the loan was not found
        Assert.ThrowsAsync<EntityNotFoundException>(async () => await _tenantFasade.GetMyLoan(loanId));
        
        // Verify that GetCurrentUser was called
        _authenticateService.Verify(o => o.GetCurrentUser(), Times.Once);
        
        // Verify that Get was called
        _loanService.Verify(o => o.Get(loanId, true), Times.Once);
    }
    
    [Test]
    public void GetMyLoan_UserIsNotTenant_ThrowsException()
    {
        // Arrange
        var loanId = 1;
        var loan = new Loan{Id = loanId, Tenant = new ApplicationUser(){Id="2"}};
        
        // Authentication service will return the user
        _authenticateService.Setup(o => o.GetCurrentUser()).ReturnsAsync(_user);
        
        // Loan service will return the loan
        _loanService.Setup(o => o.Get(loanId, true)).ReturnsAsync(loan);
        
        // Must throw UnauthorizedAccessException because the user is not the tenant
        Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await _tenantFasade.GetMyLoan(loanId));
        
        // Verify that GetCurrentUser was called
        _authenticateService.Verify(o => o.GetCurrentUser(), Times.Once);
        
        // Verify that Get was called
        _loanService.Verify(o => o.Get(loanId, true), Times.Once);
    }
    
    [Test]
    public async Task GetMyLoan_UserIsTenant_ReturnsLoan()
    {
        // Arrange
        var loanId = 1;
        var loan = new Loan{Id = loanId, Tenant = _user};
        
        // Authentication service will return the user
        _authenticateService.Setup(o => o.GetCurrentUser()).ReturnsAsync(_user);
        
        // Loan service will return the loan
        _loanService.Setup(o => o.Get(loanId, true)).ReturnsAsync(loan);
        
        // Act
        var result = await _tenantFasade.GetMyLoan(loanId);
        
        // Verify that GetCurrentUser was called
        _authenticateService.Verify(o => o.GetCurrentUser(), Times.Once);
        
        // Verify that Get was called
        _loanService.Verify(o => o.Get(loanId, true), Times.Once);

        // must be the same
        Assert.That(result, Is.EqualTo(loan));
    }
    #endregion
}
using Moq;
using PujcovadloServer.Authentication;
using PujcovadloServer.Authentication.Exceptions;
using PujcovadloServer.Business.Entities;
using PujcovadloServer.Business.Enums;
using PujcovadloServer.Business.Exceptions;
using PujcovadloServer.Business.Filters;
using PujcovadloServer.Business.Services;
using PujcovadloServer.Business.Services.Interfaces;
using PujcovadloServer.Lib;
using PujcovadloServer.Requests;

namespace Tests.Business.Facades;

public class LoanFacadeTest
{
    private LoanFacade _loanFacade;
    private Mock<LoanService> _loanService;
    private Mock<OwnerFacade> _ownerFacade;
    private Mock<TenantFacade> _tenantFacade;
    private Mock<IAuthenticateService> _authenticateService;

    private ApplicationUser _user;

    private ApplicationUser _owner;

    private ApplicationUser _tenant;

    private PujcovadloServer.Business.Entities.Item _item;

    private PujcovadloServer.Business.Entities.Loan _loan;

    [SetUp]
    public void Setup()
    {
        _loanService = new Mock<LoanService>(null, null);
        _ownerFacade = new Mock<OwnerFacade>(null, null, null);
        _tenantFacade = new Mock<TenantFacade>(null, null, null, null);
        _authenticateService = new Mock<IAuthenticateService>();

        _loanFacade = new LoanFacade(_loanService.Object, _ownerFacade.Object, _tenantFacade.Object,
            _authenticateService.Object);

        _user = new ApplicationUser() { Id = "1" };

        _owner = new ApplicationUser() { Id = "2" };

        _tenant = new ApplicationUser() { Id = "3" };

        _item = new PujcovadloServer.Business.Entities.Item
        {
            Id = 1,
            Owner = _owner
        };

        _loan = new PujcovadloServer.Business.Entities.Loan
        {
            Id = 1,
            Item = _item,
            Tenant = _tenant
        };
    }

    #region GetLoan

    [Test]
    public void GetLoan_LoanNotFound_ThrowsException()
    {
        var loanIdThatDoesNotExist = 20;

        // Loan service will return null
        _loanService.Setup(o => o.Get(loanIdThatDoesNotExist, true)).ReturnsAsync((Loan)null);

        // Must throw EntityNotFoundException because the loan was not found
        Assert.ThrowsAsync<EntityNotFoundException>(async () => await _loanFacade.GetLoan(loanIdThatDoesNotExist));

        // Verify that Get was called
        _loanService.Verify(o => o.Get(loanIdThatDoesNotExist, true), Times.Once);
    }

    [Test]
    public async Task GetLoan_LoanFound_ReturnsLoan()
    {
        // Arrange
        var loanId = 1;
        var from = DateTime.Now;
        var to = from.AddDays(2);
        var loan = new Loan
        {
            Id = loanId,
            From = from,
            To = to,
            Days = 1,
            PricePerDay = 200,
            ExpectedPrice = 200,
            RefundableDeposit = 2000,
            TenantNote = "Tenant note",
            Tenant = _user,
            Item = new Item() { Id = 1 },
            PickupProtocol = new PickupProtocol() { Id = 1 }
        };

        var expectedLoan = new Loan
        {
            Id = loanId,
            From = from,
            To = to,
            Days = 1,
            PricePerDay = 200,
            ExpectedPrice = 200,
            RefundableDeposit = 2000,
            TenantNote = "Tenant note",
            Tenant = _user,
            Item = new Item() { Id = 1 },
            PickupProtocol = new PickupProtocol() { Id = 1 }
        };

        // Loan service will return the loan
        _loanService.Setup(o => o.Get(loanId, true)).ReturnsAsync(loan);

        // Act
        var result = await _loanFacade.GetLoan(loanId);

        // Assert
        Assert.That(result.Id, Is.EqualTo(expectedLoan.Id));
        Assert.That(result.From, Is.EqualTo(expectedLoan.From));
        Assert.That(result.To, Is.EqualTo(expectedLoan.To));
        Assert.That(result.Days, Is.EqualTo(expectedLoan.Days));
        Assert.That(result.PricePerDay, Is.EqualTo(expectedLoan.PricePerDay));
        Assert.That(result.ExpectedPrice, Is.EqualTo(expectedLoan.ExpectedPrice));
        Assert.That(result.RefundableDeposit, Is.EqualTo(expectedLoan.RefundableDeposit));
        Assert.That(result.TenantNote, Is.EqualTo(expectedLoan.TenantNote));
        Assert.That(result.Tenant.Id, Is.EqualTo(expectedLoan.Tenant.Id));
        Assert.That(result.Item.Id, Is.EqualTo(expectedLoan.Item.Id));
        Assert.That(result.PickupProtocol?.Id, Is.EqualTo(expectedLoan.PickupProtocol?.Id));

        // Verify that Get was called
        _loanService.Verify(o => o.Get(loanId, true), Times.Once);
    }

    #endregion

    #region GetLoans

    [Test]
    public void GetLoans_UserIsNotAuthenticated_ThrownException()
    {
        // arrange 
        var filter = new LoanFilter { OwnerId = _owner.Id };
        var loans = new List<Loan>() { _loan };
        var paginatedLoans = new PaginatedList<Loan>(loans, loans.Count, 1, 10);

        // mock authenticate service to return owner id
        _authenticateService.Setup(o => o.GetCurrentUserId()).Throws(new NotAuthenticatedException());

        // act - exception is expected because the user is not authenticated
        Assert.ThrowsAsync<NotAuthenticatedException>(async () => await _loanFacade.GetLoans(filter));

        // Verfy that no other service was called
        _ownerFacade.Verify(f => f.GetMyLoans(filter), Times.Never);
        _tenantFacade.Verify(f => f.GetMyLoans(filter), Times.Never);
        _loanService.Verify(f => f.GetLoansByUserId(_owner.Id, filter), Times.Never);
    }

    [Test]
    public async Task GetLoans_UserIsOwnerAndFilterByOwner_OwnerFacadeUsed()
    {
        // arrange 
        var filter = new LoanFilter { OwnerId = _owner.Id };
        var loans = new List<Loan>() { _loan };
        var paginatedLoans = new PaginatedList<Loan>(loans, loans.Count, 1, 10);

        // mock authenticate service to return owner id
        _authenticateService.Setup(o => o.GetCurrentUserId()).Returns(_owner.Id);

        // mock owner facade to return loans
        _ownerFacade.Setup(f => f.GetMyLoans(filter)).ReturnsAsync(paginatedLoans);

        // act
        var result = await _loanFacade.GetLoans(filter);

        // assert 
        Assert.That(result.Count, Is.EqualTo(paginatedLoans.Count));
        Assert.That(result.PageIndex, Is.EqualTo(paginatedLoans.PageIndex));
        Assert.That(result.TotalPages, Is.EqualTo(paginatedLoans.TotalPages));

        // verify items
        for (var i = 0; i < result.Count; i++)
        {
            Assert.That(result[i].Id, Is.EqualTo(paginatedLoans[i].Id));
        }

        // verify that owner facade was called
        _ownerFacade.Verify(f => f.GetMyLoans(filter), Times.Once);

        // verify that no other service was called
        _tenantFacade.Verify(f => f.GetMyLoans(filter), Times.Never);
        _loanService.Verify(f => f.GetLoansByUserId(_owner.Id, filter), Times.Never);
    }

    [Test]
    public async Task GetLoans_UserIsOwnerButFilterByTenant_TenantFacadeUsedAndEmptyResultReturned()
    {
        // arrange 
        var filter = new LoanFilter() { TenantId = _owner.Id };
        var loans = new List<Loan>() { };
        var paginatedLoans = new PaginatedList<Loan>(loans, loans.Count, 1, 10);

        // mock authenticate service to return owner id
        _authenticateService.Setup(o => o.GetCurrentUserId()).Returns(_owner.Id);

        // mock loan service to return loans
        _tenantFacade.Setup(f => f.GetMyLoans(filter)).ReturnsAsync(paginatedLoans);

        // act
        var result = await _loanFacade.GetLoans(filter);

        // assert 
        Assert.That(result.Count, Is.EqualTo(paginatedLoans.Count));
        Assert.That(result.PageIndex, Is.EqualTo(paginatedLoans.PageIndex));
        Assert.That(result.TotalPages, Is.EqualTo(paginatedLoans.TotalPages));

        // verify that loan service was called
        _tenantFacade.Verify(f => f.GetMyLoans(filter), Times.Once);

        // verify that no other service was called
        _ownerFacade.Verify(f => f.GetMyLoans(filter), Times.Never);
        _loanService.Verify(f => f.GetLoansByUserId(_owner.Id, filter), Times.Never);
    }

    [Test]
    public async Task GetLoans_UserIsOwnerButNoFilterByTenantOrOwner_LoanServiceUsed()
    {
        // arrange 
        var filter = new LoanFilter();
        var loans = new List<Loan>() { _loan };
        var paginatedLoans = new PaginatedList<Loan>(loans, loans.Count, 1, 10);

        // mock authenticate service to return owner id
        _authenticateService.Setup(o => o.GetCurrentUserId()).Returns(_owner.Id);

        // mock loan service to return loans
        _loanService.Setup(f => f.GetLoansByUserId(_owner.Id, filter)).ReturnsAsync(paginatedLoans);

        // act
        var result = await _loanFacade.GetLoans(filter);

        // assert 
        Assert.That(result.Count, Is.EqualTo(paginatedLoans.Count));
        Assert.That(result.PageIndex, Is.EqualTo(paginatedLoans.PageIndex));
        Assert.That(result.TotalPages, Is.EqualTo(paginatedLoans.TotalPages));

        // verify items
        for (var i = 0; i < result.Count; i++)
        {
            Assert.That(result[i].Id, Is.EqualTo(paginatedLoans[i].Id));
        }

        // verify that loan service was called
        _loanService.Verify(f => f.GetLoansByUserId(_owner.Id, filter), Times.Once);

        // verify that no other service was called
        _ownerFacade.Verify(f => f.GetMyLoans(filter), Times.Never);
        _tenantFacade.Verify(f => f.GetMyLoans(filter), Times.Never);
    }

    //

    [Test]
    public async Task GetLoans_UserIsTenantAndFilterByTenant_TenantFacadeUsed()
    {
        // arrange 
        var filter = new LoanFilter { TenantId = _tenant.Id };
        var loans = new List<Loan>() { _loan };
        var paginatedLoans = new PaginatedList<Loan>(loans, loans.Count, 1, 10);

        // mock authenticate service to return owner id
        _authenticateService.Setup(o => o.GetCurrentUserId()).Returns(_tenant.Id);

        // mock owner facade to return loans
        _tenantFacade.Setup(f => f.GetMyLoans(filter)).ReturnsAsync(paginatedLoans);

        // act
        var result = await _loanFacade.GetLoans(filter);

        // assert 
        Assert.That(result.Count, Is.EqualTo(paginatedLoans.Count));
        Assert.That(result.PageIndex, Is.EqualTo(paginatedLoans.PageIndex));
        Assert.That(result.TotalPages, Is.EqualTo(paginatedLoans.TotalPages));

        // verify items
        for (var i = 0; i < result.Count; i++)
        {
            Assert.That(result[i].Id, Is.EqualTo(paginatedLoans[i].Id));
        }

        // verify that owner facade was called
        _tenantFacade.Verify(f => f.GetMyLoans(filter), Times.Once);

        // verify that no other service was called
        _ownerFacade.Verify(f => f.GetMyLoans(filter), Times.Never);
        _loanService.Verify(f => f.GetLoansByUserId(_owner.Id, filter), Times.Never);
    }

    [Test]
    public async Task GetLoans_UserIsTenantButFilterByOwner_OwnerFacadeUsedAndEmptyResultReturned()
    {
        // arrange 
        var filter = new LoanFilter() { OwnerId = _tenant.Id };
        var loans = new List<Loan>() { };
        var paginatedLoans = new PaginatedList<Loan>(loans, loans.Count, 1, 10);

        // mock authenticate service to return owner id
        _authenticateService.Setup(o => o.GetCurrentUserId()).Returns(_tenant.Id);

        // mock loan service to return loans
        _ownerFacade.Setup(f => f.GetMyLoans(filter)).ReturnsAsync(paginatedLoans);

        // act
        var result = await _loanFacade.GetLoans(filter);

        // assert 
        Assert.That(result.Count, Is.EqualTo(paginatedLoans.Count));
        Assert.That(result.PageIndex, Is.EqualTo(paginatedLoans.PageIndex));
        Assert.That(result.TotalPages, Is.EqualTo(paginatedLoans.TotalPages));

        // verify that loan service was called
        _ownerFacade.Verify(f => f.GetMyLoans(filter), Times.Once);

        // verify that no other service was called
        _tenantFacade.Verify(f => f.GetMyLoans(filter), Times.Never);
        _loanService.Verify(f => f.GetLoansByUserId(_owner.Id, filter), Times.Never);
    }

    [Test]
    public async Task GetLoans_UserIsTenantButNoFilterByTenantOrOwner_LoanServiceUsed()
    {
        // arrange 
        var filter = new LoanFilter();
        var loans = new List<Loan>() { _loan };
        var paginatedLoans = new PaginatedList<Loan>(loans, loans.Count, 1, 10);

        // mock authenticate service to return owner id
        _authenticateService.Setup(o => o.GetCurrentUserId()).Returns(_tenant.Id);

        // mock loan service to return loans
        _loanService.Setup(f => f.GetLoansByUserId(_tenant.Id, filter)).ReturnsAsync(paginatedLoans);

        // act
        var result = await _loanFacade.GetLoans(filter);

        // assert 
        Assert.That(result.Count, Is.EqualTo(paginatedLoans.Count));
        Assert.That(result.PageIndex, Is.EqualTo(paginatedLoans.PageIndex));
        Assert.That(result.TotalPages, Is.EqualTo(paginatedLoans.TotalPages));

        // verify items
        for (var i = 0; i < result.Count; i++)
        {
            Assert.That(result[i].Id, Is.EqualTo(paginatedLoans[i].Id));
        }

        // verify that loan service was called
        _loanService.Verify(f => f.GetLoansByUserId(_tenant.Id, filter), Times.Once);

        // verify that no other service was called
        _ownerFacade.Verify(f => f.GetMyLoans(filter), Times.Never);
        _tenantFacade.Verify(f => f.GetMyLoans(filter), Times.Never);
    }

    #endregion

    #region Update

    [Test]
    public void UpdateMyLoan_UserNotAuthenticated_ThrowsException()
    {
        // Arrange
        var request = new LoanUpdateRequest { Id = 10, Status = LoanStatus.Active };

        // Arrange
        _authenticateService.Setup(x => x.GetCurrentUserId()).Throws(new NotAuthenticatedException());

        // Run the operation
        Assert.ThrowsAsync<NotAuthenticatedException>(() => _loanFacade.UpdateLoan(_loan, request));

        // Check that no update method was called
        _loanService.Verify(x => x.Update(_loan), Times.Never);
        _ownerFacade.Verify(x => x.UpdateMyLoan(_loan, request), Times.Never);
        _tenantFacade.Verify(x => x.UpdateMyLoan(_loan, request), Times.Never);
    }

    [Test]
    public async Task UpdateMyLoan_UserIsTheOwner_CallsOwnerFacadeUpdate()
    {
        // Arrange
        var request = new LoanUpdateRequest { Id = 10, Status = LoanStatus.Active };

        // Arrange
        _authenticateService.Setup(x => x.GetCurrentUserId()).Returns(_owner.Id);

        // Run the operation
        await _loanFacade.UpdateLoan(_loan, request);

        // Check that owner facade was called
        _ownerFacade.Verify(x => x.UpdateMyLoan(_loan, request), Times.Once);

        // Check that tenant facade was not called
        _tenantFacade.Verify(x => x.UpdateMyLoan(_loan, request), Times.Never);
    }

    [Test]
    public async Task UpdateMyLoan_UserIsTheTenant_CallsTenantFacadeUpdate()
    {
        // Arrange
        var request = new LoanUpdateRequest { Id = 10, Status = LoanStatus.Active };

        // Arrange
        _authenticateService.Setup(x => x.GetCurrentUserId()).Returns(_tenant.Id);

        // Run the operation
        await _loanFacade.UpdateLoan(_loan, request);

        // Check that tenant facade was called
        _tenantFacade.Verify(x => x.UpdateMyLoan(_loan, request), Times.Once);

        // Check that owner facade was not called
        _ownerFacade.Verify(x => x.UpdateMyLoan(_loan, request), Times.Never);
    }

    #endregion
}
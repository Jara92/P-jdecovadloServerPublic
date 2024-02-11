using AutoMapper;
using Moq;
using PujcovadloServer.Authentication;
using PujcovadloServer.Authentication.Exceptions;
using PujcovadloServer.AuthorizationHandlers.Exceptions;
using PujcovadloServer.Business.Entities;
using PujcovadloServer.Business.Enums;
using PujcovadloServer.Business.Exceptions;
using PujcovadloServer.Business.Filters;
using PujcovadloServer.Business.Services;
using PujcovadloServer.Business.Services.Interfaces;
using PujcovadloServer.Business.States.Loan;
using PujcovadloServer.Lib;
using PujcovadloServer.Requests;

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
        _itemService = new Mock<ItemService>(null);
        _authenticateService = new Mock<IAuthenticateService>();
        _mapper = new Mock<IMapper>();

        _tenantFasade = new TenantFacade(_loanService.Object, _itemService.Object, _authenticateService.Object,
            _mapper.Object);

        _user = new ApplicationUser() { Id = "1" };
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

        // Verify that GetLoansByTenant was not called
        _loanService.Verify(l => l.GetLoansByTenant(It.IsAny<ApplicationUser>(), It.IsAny<LoanFilter>()), Times.Never);
    }

    [Test]
    public async Task GetMyLoans_AuthenticatedUser_ReturnsLoans()
    {
        var loanFilter = new LoanFilter() { Page = 1, PageSize = 10 };
        var loans = new List<Loan>()
        {
            new Loan { Id = 1, Tenant = _user },
            new Loan { Id = 2, Tenant = _user },
            new Loan { Id = 3, Tenant = _user }
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

        // Verify that GetLoan was not called
        _loanService.Verify(l => l.Get(It.IsAny<int>(), true), Times.Never);
    }

    [Test]
    public void GetMyLoan_LoanNotFound_ThrowsException()
    {
        // Arrange
        var loanId = 1;
        var loan = new Loan { Id = loanId, Tenant = _user };

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
        var loan = new Loan { Id = loanId, Tenant = new ApplicationUser() { Id = "2" } };

        // Authentication service will return the user
        _authenticateService.Setup(o => o.GetCurrentUser()).ReturnsAsync(_user);

        // Loan service will return the loan
        _loanService.Setup(o => o.Get(loanId, true)).ReturnsAsync(loan);

        // Must throw ForbiddenAccessException because the user is not the tenant
        Assert.ThrowsAsync<ForbiddenAccessException>(async () => await _tenantFasade.GetMyLoan(loanId));

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
        var loan = new Loan { Id = loanId, Tenant = _user };

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
        Assert.That(result.Id, Is.EqualTo(loan.Id));
        Assert.That(result.Tenant, Is.EqualTo(loan.Tenant));
    }

    #endregion

    #region CreateLoan

    [Test]
    public void CreateLoan_UnauthenticatedUser_ThrowsException()
    {
        // Authentication service will throw NotAuthenticatedException
        _authenticateService.Setup(o => o.GetCurrentUser()).Throws<NotAuthenticatedException>();

        // Must throw NotAuthenticatedException because no user is authenticated
        Assert.ThrowsAsync<NotAuthenticatedException>(async () =>
            await _tenantFasade.CreateLoan(new TenantLoanRequest()));

        // Verify that GetCurrentUser was called
        _authenticateService.Verify(o => o.GetCurrentUser(), Times.Once);

        // Verfiy that create was not called
        _loanService.Verify(l => l.Create(It.IsAny<Loan>()), Times.Never);
    }

    [Test]
    public void CreateLoan_ItemNotFound_ThrowsException()
    {
        // Arrange
        var request = new TenantLoanRequest() { Item = new ItemRequest() { Id = 1 } };

        // Authentication service will return the user
        _authenticateService.Setup(o => o.GetCurrentUser()).ReturnsAsync(_user);

        // Item service will return null
        _itemService.Setup(o => o.Get(request.Item.Id, true)).ReturnsAsync((Item)null);

        // Must throw EntityNotFoundException because the item was not found
        Assert.ThrowsAsync<EntityNotFoundException>(async () => await _tenantFasade.CreateLoan(request));

        // Verify that GetCurrentUser was called
        _authenticateService.Verify(o => o.GetCurrentUser(), Times.Once);

        // Verify that Get was called
        _itemService.Verify(o => o.Get(request.Item.Id, true), Times.Once);

        // Verfiy that create was not called
        _loanService.Verify(l => l.Create(It.IsAny<Loan>()), Times.Never);
    }

    [Test]
    public async Task CreateLoan_UserIsTenant_ReturnsLoan()
    {
        var dateFrom = DateTime.Now;
        var dateTo = dateFrom.AddDays(1);

        // Simulate the request
        var request = new TenantLoanRequest()
        {
            Item = new ItemRequest { Id = 1 },
            From = dateFrom,
            To = dateTo,
            TenantNote = "Note"
        };

        // Expected item 
        var item = new Item
        {
            Id = request.Id,
            PricePerDay = 100,
            RefundableDeposit = 2000
        };

        var mappedLoan = new Loan()
        {
            From = dateFrom,
            To = dateTo,
            TenantNote = request.TenantNote
        };

        // Expected loan
        var expectedLoan = new Loan
        {
            From = dateFrom,
            To = dateTo,
            Tenant = _user,
            Item = item,
            PricePerDay = item.PricePerDay,
            RefundableDeposit = item.RefundableDeposit,
            Days = 1,
            ExpectedPrice = 1 * item.PricePerDay,
            TenantNote = request.TenantNote
        };

        // Authentication service will return the user
        _authenticateService.Setup(o => o.GetCurrentUser()).ReturnsAsync(_user);

        // Item service will return the item
        _itemService.Setup(o => o.Get(request.Item.Id, true)).ReturnsAsync(item);

        // Mapper will map the request to loan
        _mapper.Setup(o => o.Map<Loan>(request)).Returns(mappedLoan);

        // Loan service will return the loan
        _loanService.Setup(o => o.Create(expectedLoan)).Returns(Task.CompletedTask);

        // Act
        var result = await _tenantFasade.CreateLoan(request);

        // Verify that GetCurrentUser was called
        _authenticateService.Verify(o => o.GetCurrentUser(), Times.Once);

        // Verify that Get was called
        _itemService.Verify(o => o.Get(request.Item.Id, true), Times.Once);

        // Verify that create was called
        _loanService.Verify(l => l.Create(mappedLoan), Times.Once);

        // must be the same
        Assert.That(result.Days, Is.EqualTo(expectedLoan.Days));
        Assert.That(result.ExpectedPrice, Is.EqualTo(expectedLoan.ExpectedPrice));
        Assert.That(result.From, Is.EqualTo(expectedLoan.From));
        Assert.That(result.To, Is.EqualTo(expectedLoan.To));
        Assert.That(result.Item, Is.EqualTo(expectedLoan.Item));
        Assert.That(result.PricePerDay, Is.EqualTo(expectedLoan.PricePerDay));
        Assert.That(result.RefundableDeposit, Is.EqualTo(expectedLoan.RefundableDeposit));
        Assert.That(result.Tenant, Is.EqualTo(expectedLoan.Tenant));
        Assert.That(result.TenantNote, Is.EqualTo(expectedLoan.TenantNote));
    }

    [Test]
    public async Task CreateLoan_UserIsTenantAndItemsOwner_ThrowsException()
    {
        var dateFrom = DateTime.Now;
        var dateTo = dateFrom.AddDays(1);

        // Simulate the request
        var request = new TenantLoanRequest()
        {
            Item = new ItemRequest { Id = 1 },
            From = dateFrom,
            To = dateTo,
            TenantNote = "Note"
        };

        // Expected item 
        var item = new Item
        {
            Id = request.Id,
            PricePerDay = 100,
            RefundableDeposit = 2000,
            Owner = _user
        };

        var mappedLoan = new Loan()
        {
            From = dateFrom,
            To = dateTo,
            TenantNote = request.TenantNote
        };

        // Expected loan
        var expectedLoan = new Loan
        {
            From = dateFrom,
            To = dateTo,
            Tenant = _user,
            Item = item,
            PricePerDay = item.PricePerDay,
            RefundableDeposit = item.RefundableDeposit,
            Days = 1,
            ExpectedPrice = 1 * item.PricePerDay,
            TenantNote = request.TenantNote
        };

        // Authentication service will return the user
        _authenticateService.Setup(o => o.GetCurrentUser()).ReturnsAsync(_user);

        // Item service will return the item
        _itemService.Setup(o => o.Get(request.Item.Id, true)).ReturnsAsync(item);

        // Mapper will map the request to loan
        _mapper.Setup(o => o.Map<Loan>(request)).Returns(mappedLoan);

        // Loan service will return the loan
        _loanService.Setup(o => o.Create(expectedLoan)).Returns(Task.CompletedTask);

        // Must throw OperationNotAllowedException because the user is the owner of the item
        Assert.ThrowsAsync<OperationNotAllowedException>(async () => await _tenantFasade.CreateLoan(request));
    }

    [Test]
    public async Task CreateLoan_UserIsTenantButItemIsNotPublic_ThrowsException()
    {
        var dateFrom = DateTime.Now;
        var dateTo = dateFrom.AddDays(1);

        // Simulate the request
        var request = new TenantLoanRequest()
        {
            Item = new ItemRequest { Id = 1 },
            From = dateFrom,
            To = dateTo,
            TenantNote = "Note"
        };

        // Expected item 
        var item = new Item
        {
            Id = request.Id,
            PricePerDay = 100,
            RefundableDeposit = 2000,
            Status = ItemStatus.Approving
        };

        var mappedLoan = new Loan()
        {
            From = dateFrom,
            To = dateTo,
            TenantNote = request.TenantNote
        };

        // Expected loan
        var expectedLoan = new Loan
        {
            From = dateFrom,
            To = dateTo,
            Tenant = _user,
            Item = item,
            PricePerDay = item.PricePerDay,
            RefundableDeposit = item.RefundableDeposit,
            Days = 1,
            ExpectedPrice = 1 * item.PricePerDay,
            TenantNote = request.TenantNote
        };

        // Authentication service will return the user
        _authenticateService.Setup(o => o.GetCurrentUser()).ReturnsAsync(_user);

        // Item service will return the item
        _itemService.Setup(o => o.Get(request.Item.Id, true)).ReturnsAsync(item);

        // Mapper will map the request to loan
        _mapper.Setup(o => o.Map<Loan>(request)).Returns(mappedLoan);

        // Loan service will return the loan
        _loanService.Setup(o => o.Create(expectedLoan)).Returns(Task.CompletedTask);

        // Must throw OperationNotAllowedException because the user is the owner of the item
        Assert.ThrowsAsync<OperationNotAllowedException>(async () => await _tenantFasade.CreateLoan(request));
    }

    #endregion

    #region GetLoanDays

    [Test]
    public void SetLoanDays_SameDate_ReturnsOneDay()
    {
        var loan = new Loan { From = DateTime.Now, To = DateTime.Now };

        var days = _tenantFasade.GetLoanDays(loan);

        // Should be one day even if From and To are the same
        Assert.That(days, Is.EqualTo(1));
    }

    [Test]
    public void SetLoanDays_SameDateDifferentTime_ReturnsOneDay()
    {
        var from = DateTime.Now;
        var to = from;

        var loan = new Loan { From = from, To = to };

        var days = _tenantFasade.GetLoanDays(loan);

        // Should be one day
        Assert.That(days, Is.EqualTo(1));
    }

    [Test]
    public void SetLoanDays_DifferentDates_ReturnsDays()
    {
        var dateFrom = DateTime.Now;
        var dateTo = dateFrom.AddDays(1);

        var loan = new Loan { From = dateFrom, To = dateTo };

        var days = _tenantFasade.GetLoanDays(loan);

        // Should be one day
        Assert.That(days, Is.EqualTo(1));
    }

    [Test]
    public void SetLoanDays_DifferentDatesDifferentTime_ReturnsDays()
    {
        var dateFrom = DateTime.Now;
        var dateTo = dateFrom.AddDays(1);

        var loan = new Loan { From = dateFrom, To = dateTo };

        var days = _tenantFasade.GetLoanDays(loan);

        // Should be one day because time is not considered
        Assert.That(days, Is.EqualTo(1));
    }

    [Test]
    public void SetLoanDays_DifferentDates_ReturnsDays2()
    {
        var dateFrom = DateTime.Now;
        var dateTo = dateFrom.AddDays(7);

        var loan = new Loan { From = dateFrom, To = dateTo };

        var days = _tenantFasade.GetLoanDays(loan);

        // Should be two days because time is not considered
        Assert.That(days, Is.EqualTo(7));
    }

    #endregion

    #region GetLoanExpectedPrice

    [Test]
    public void SetLoanExpectedPrice_ZeroDays_ThrowsException()
    {
        var loan = new Loan { Days = 0, PricePerDay = 100 };

        // Must throw ArgumentException becuase days are zero
        Assert.Throws<ArgumentException>(() => _tenantFasade.GetLoanExpectedPrice(loan));
    }

    [Test]
    public void SetLoanExpectedPrice_OneDay_ReturnsPricePerDay()
    {
        var loan = new Loan { Days = 1, PricePerDay = 100 };

        var price = _tenantFasade.GetLoanExpectedPrice(loan);

        // Should be the same as price per day
        Assert.That(price, Is.EqualTo(100));
    }

    [Test]
    public void SetLoanExpectedPrice_TwoDays_ReturnsPricePerDayTimesTwo()
    {
        var loan = new Loan { Days = 2, PricePerDay = 100 };

        var price = _tenantFasade.GetLoanExpectedPrice(loan);

        // Should be the same as price per day
        Assert.That(price, Is.EqualTo(200));
    }

    [Test]
    public void SetLoanExpectedPrice_ThreeDays_ReturnsPricePerDayTimesSeven()
    {
        var loan = new Loan { Days = 7, PricePerDay = 100 };

        var price = _tenantFasade.GetLoanExpectedPrice(loan);

        // Should be the same as price per day
        Assert.That(price, Is.EqualTo(700));
    }

    #endregion

    #region UpdateMyLoan

    [Test]
    public void UpdateMyLoan_UnauthenticatedUser_ThrowsException()
    {
        // Authentication service will throw NotAuthenticatedException
        _authenticateService.Setup(o => o.GetCurrentUser()).Throws<NotAuthenticatedException>();

        // Must throw NotAuthenticatedException because no user is authenticated
        Assert.ThrowsAsync<NotAuthenticatedException>(async () =>
            await _tenantFasade.UpdateMyLoan(new Loan(), new TenantLoanRequest()));

        // Verify that GetCurrentUser was called
        _authenticateService.Verify(o => o.GetCurrentUser(), Times.Once);

        // Verfiy that update was not called
        _loanService.Verify(l => l.Update(It.IsAny<Loan>()), Times.Never);
    }

    [Test]
    public void UpdateMyLoan_UserIsNotTenant_ThrowsException()
    {
        // Arrange
        var loan = new Loan { Tenant = new ApplicationUser() { Id = "2" } };

        // Authentication service will return the user
        _authenticateService.Setup(o => o.GetCurrentUser()).ReturnsAsync(_user);

        // Must throw ForbiddenAccessException because the user is not the tenant
        Assert.ThrowsAsync<ForbiddenAccessException>(async () =>
            await _tenantFasade.UpdateMyLoan(loan, new TenantLoanRequest()));

        // Verify that GetCurrentUser was called
        _authenticateService.Verify(o => o.GetCurrentUser(), Times.Once);

        // Verfiy that update was not called
        _loanService.Verify(l => l.Update(It.IsAny<Loan>()), Times.Never);
    }

    [Test]
    public void UpdateMyLoan_UserIsTenantAndNotUpdatedStatus_UpdateLoan()
    {
        var loan = new Loan { Tenant = _user, Status = LoanStatus.Inquired };

        var newStatus = LoanStatus.Inquired;
        var request = new TenantLoanRequest { Status = newStatus };

        var mockState = new Mock<ILoanState>();

        // Authentication service will return the user
        _authenticateService.Setup(o => o.GetCurrentUser()).ReturnsAsync(_user);

        // Loan service will return the loan
        _loanService.Setup(o => o.Update(loan)).Returns(Task.CompletedTask);

        // Returns the state
        _loanService.Setup(o => o.GetState(loan)).Returns(mockState.Object);

        // Act
        var result = _tenantFasade.UpdateMyLoan(loan, request);

        // Verify that GetCurrentUser was called
        _authenticateService.Verify(o => o.GetCurrentUser(), Times.Once);

        // Check that the state change by tenant was NOT called because the status is the same
        mockState.Verify(o => o.HandleTenant(loan, newStatus), Times.Never);

        // Check that the state change by owner was NOT called
        mockState.Verify(o => o.HandleOwner(loan, newStatus), Times.Never);

        // Verify that update was called
        _loanService.Verify(l => l.Update(loan), Times.Once);
    }

    [Test]
    public void UpdateMyLoan_UserIsTenantAndUpdatedStatus_UpdatesLoan()
    {
        var loan = new Loan { Tenant = _user, Status = LoanStatus.Inquired };

        var newStatus = LoanStatus.Accepted;
        var request = new TenantLoanRequest { Status = newStatus };

        // Mock the state so we can verify that the state change was called
        var mockState = new Mock<ILoanState>();

        // Authentication service will return the user
        _authenticateService.Setup(o => o.GetCurrentUser()).ReturnsAsync(_user);

        // Loan service will return the loan
        _loanService.Setup(o => o.Update(loan)).Returns(Task.CompletedTask);

        // Returns the state
        _loanService.Setup(o => o.GetState(loan)).Returns(mockState.Object);

        // Act
        var result = _tenantFasade.UpdateMyLoan(loan, request);

        // Verify that GetCurrentUser was called
        _authenticateService.Verify(o => o.GetCurrentUser(), Times.Once);

        // Check that the state change by tenant was called because the status was changed
        mockState.Verify(o => o.HandleTenant(loan, newStatus), Times.Once);

        // Check that the state change by owner was NOT called
        mockState.Verify(o => o.HandleOwner(loan, newStatus), Times.Never);

        // Verify that update was called
        _loanService.Verify(l => l.Update(loan), Times.Once);
    }

    [Test]
    public async Task UpdateMyLoan_UserIsTenantButTriesToUpdateDisallowedFields_LoanUpdatedWithoutTheFields()
    {
        // The item
        var item = new Item
        {
            Id = 1
        };

        var from = DateTime.Now;
        var to = from.AddDays(2);

        // Loan to be updated
        var loan = new Loan
        {
            Id = 1,
            Tenant = _user,
            Status = LoanStatus.Inquired,
            From = from,
            To = to,
            Days = 2,
            ExpectedPrice = 200,
            PricePerDay = 100,
            RefundableDeposit = 2000,
            Item = item
        };

        // Expected result
        var expectedLoan = new Loan
        {
            Id = 1,
            Tenant = _user,
            Status = LoanStatus.Inquired,
            From = from,
            To = to,
            Days = 2,
            ExpectedPrice = 200,
            PricePerDay = 100,
            RefundableDeposit = 2000,
            Item = item
        };

        // Request with filled fields which are not allowed to be updated
        var request = new TenantLoanRequest
        {
            Id = 1,
            From = DateTime.Now.AddDays(5),
            To = DateTime.Now.AddDays(10),
            Item = new ItemRequest { Id = 2 }
        };

        // Authentication service will return the user
        _authenticateService.Setup(o => o.GetCurrentUser()).ReturnsAsync(_user);

        // Loan service will return the loan
        _loanService.Setup(o => o.Update(loan)).Returns(Task.CompletedTask);

        // Act
        await _tenantFasade.UpdateMyLoan(loan, request);

        // Verify that GetCurrentUser was called
        _authenticateService.Verify(o => o.GetCurrentUser(), Times.Once);

        // Verify that update was called
        _loanService.Verify(l => l.Update(loan), Times.Once);

        // Check that the loan was updated without the fields
        Assert.That(loan.Id, Is.EqualTo(expectedLoan.Id));
        Assert.That(loan.Tenant, Is.EqualTo(expectedLoan.Tenant));

        // changing status is allowed but not expected in this test
        Assert.That(loan.Status, Is.EqualTo(expectedLoan.Status));
        Assert.That(loan.From, Is.EqualTo(expectedLoan.From));
        Assert.That(loan.To, Is.EqualTo(expectedLoan.To));
        Assert.That(loan.Days, Is.EqualTo(expectedLoan.Days));
        Assert.That(loan.ExpectedPrice, Is.EqualTo(expectedLoan.ExpectedPrice));
        Assert.That(loan.PricePerDay, Is.EqualTo(expectedLoan.PricePerDay));
        Assert.That(loan.RefundableDeposit, Is.EqualTo(expectedLoan.RefundableDeposit));
        Assert.That(loan.Item, Is.EqualTo(expectedLoan.Item));
    }

    #endregion
}
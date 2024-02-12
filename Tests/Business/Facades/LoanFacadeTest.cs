using Moq;
using PujcovadloServer.Authentication;
using PujcovadloServer.Business.Entities;
using PujcovadloServer.Business.Exceptions;
using PujcovadloServer.Business.Services;

namespace Tests.Business.Facades;

public class LoanFacadeTest
{
    private LoanFacade _loanFacade;
    private Mock<LoanService> _loanService;
    private Mock<PickupProtocolService> _pickupProtocolService;

    private ApplicationUser _user;

    [SetUp]
    public void Setup()
    {
        _loanService = new Mock<LoanService>(null, null);
        _pickupProtocolService = new Mock<PickupProtocolService>(null);

        _loanFacade = new LoanFacade(_loanService.Object, _pickupProtocolService.Object);

        _user = new ApplicationUser() { Id = "1" };
    }

    #region GetLoan

    [Test]
    public void GetLoan_LoanNotFound_ThrowsException()
    {
        // Arrange
        var loanId = 1;
        var loan = new Loan { Id = loanId, Tenant = _user };

        // Loan service will return null
        _loanService.Setup(o => o.Get(loanId, true)).ReturnsAsync((Loan)null);

        // Must throw EntityNotFoundException because the loan was not found
        Assert.ThrowsAsync<EntityNotFoundException>(async () => await _loanFacade.GetLoan(loanId));

        // Verify that Get was called
        _loanService.Verify(o => o.Get(loanId, true), Times.Once);
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
}
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

    #endregion
}
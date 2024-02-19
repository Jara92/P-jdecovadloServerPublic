using AutoMapper;
using Moq;
using PujcovadloServer;
using PujcovadloServer.Authentication;
using PujcovadloServer.Business.Entities;
using PujcovadloServer.Business.Enums;
using PujcovadloServer.Business.Services;
using PujcovadloServer.Business.Services.Interfaces;
using PujcovadloServer.Business.States.Loan;
using PujcovadloServer.Requests;

namespace Tests.Business.Facades;

public class OwnerFacadeTest
{
    private OwnerFacade _ownerFasade;
    private Mock<LoanService> _loanService;
    private Mock<IAuthenticateService> _authenticateService;
    private Mock<IMapper> _mapper;
    private Mock<PujcovadloServerConfiguration> _configuration;

    private ApplicationUser _user;
    private ApplicationUser _owner;
    private Loan _loan;

    [SetUp]
    public void Setup()
    {
        _loanService = new Mock<LoanService>(null, null);
        _authenticateService = new Mock<IAuthenticateService>();
        _mapper = new Mock<IMapper>();
        _configuration = new Mock<PujcovadloServerConfiguration>(null);

        _ownerFasade = new OwnerFacade(_loanService.Object, _authenticateService.Object, _configuration.Object);

        _user = new ApplicationUser() { Id = "1" };
        _owner = new ApplicationUser() { Id = "2" };

        var from = DateTime.Now;

        _loan = new Loan()
        {
            Id = 1,
            Item = new Item() { Owner = _owner },
            From = from,
            To = from.AddDays(1),
            Days = 1,
            PricePerDay = 100,
            ExpectedPrice = 100,
            RefundableDeposit = 2000,
        };
    }


    #region UpdateMyLoan

    [Test]
    public async Task UpdateMyLoan_StatusTransition_Success()
    {
        // Arrange
        _loan.Status = LoanStatus.Accepted;
        var request = new LoanRequest() { Id = 10, Status = LoanStatus.Active };
        var stateMock = new Mock<AcceptedLoanState>();

        // Arrange
        _loanService.Setup(x => x.GetState(_loan)).Returns(stateMock.Object);

        // Run the operation
        await _ownerFasade.UpdateMyLoan(_loan, request);

        // Make sure that state was called
        stateMock.Verify(x => x.HandleOwner(_loan, request.Status.Value), Times.Once);

        // Make sure that status change by tenant was not called
        stateMock.Verify(x => x.HandleTenant(_loan, request.Status.Value), Times.Never);

        // Make sure that update method was called
        _loanService.Verify(x => x.Update(_loan), Times.Once);
    }

    [Test]
    public async Task UpdateMyLoan_StatusNotChanged_Success()
    {
        // Arrange
        _loan.Status = LoanStatus.Accepted;
        var request = new LoanRequest() { Id = 10, Status = LoanStatus.Accepted };
        var stateMock = new Mock<AcceptedLoanState>();

        // Arrange
        _loanService.Setup(x => x.GetState(_loan)).Returns(stateMock.Object);

        // Run the operation
        await _ownerFasade.UpdateMyLoan(_loan, request);

        // Make sure that state was not called
        stateMock.Verify(x => x.HandleOwner(_loan, request.Status.Value), Times.Once);

        // Make sure that status change by tenant was not called
        stateMock.Verify(x => x.HandleTenant(_loan, request.Status.Value), Times.Never);

        // Make sure that update method was not called
        _loanService.Verify(x => x.Update(_loan), Times.Once);
    }

    [Test]
    public async Task UpdateMyLoan_StatusNull_Success()
    {
        // Arrange
        _loan.Status = LoanStatus.Accepted;
        var request = new LoanRequest() { Id = 10 };
        var stateMock = new Mock<AcceptedLoanState>();

        // Run the operation
        await _ownerFasade.UpdateMyLoan(_loan, request);

        // Make sure that state change was NOT called
        stateMock.Verify(x => x.HandleOwner(_loan, It.IsAny<LoanStatus>()), Times.Never);

        // Make sure that state change by tenant was NOT called
        stateMock.Verify(x => x.HandleTenant(_loan, It.IsAny<LoanStatus>()), Times.Never);

        // Make sure that update method was called
        _loanService.Verify(x => x.Update(_loan), Times.Once);
    }

    [Test]
    public async Task UpdateMyLoan_StatusNull_CheckUpdatedAttributes()
    {
        var expectedLoan = new Loan()
        {
            Id = _loan.Id,
            Item = _loan.Item,
            From = _loan.From,
            To = _loan.To,
            Days = _loan.Days,
            PricePerDay = _loan.PricePerDay,
            ExpectedPrice = _loan.ExpectedPrice,
            RefundableDeposit = _loan.RefundableDeposit,
        };

        // Arrange
        _loan.Status = LoanStatus.Accepted;
        var request = new LoanRequest() { Id = 10 };
        var stateMock = new Mock<AcceptedLoanState>();

        // Run the operation
        await _ownerFasade.UpdateMyLoan(_loan, request);

        // Make sure that update method was called
        _loanService.Verify(x => x.Update(_loan), Times.Once);

        // Check that the updated attributes are correct
        Assert.That(_loan.Id, Is.EqualTo(expectedLoan.Id));
        Assert.That(_loan.Item.Id, Is.EqualTo(expectedLoan.Item.Id));
        Assert.That(_loan.From, Is.EqualTo(expectedLoan.From));
        Assert.That(_loan.To, Is.EqualTo(expectedLoan.To));
        Assert.That(_loan.Days, Is.EqualTo(expectedLoan.Days));
        Assert.That(_loan.PricePerDay, Is.EqualTo(expectedLoan.PricePerDay));
        Assert.That(_loan.ExpectedPrice, Is.EqualTo(expectedLoan.ExpectedPrice));
        Assert.That(_loan.RefundableDeposit, Is.EqualTo(expectedLoan.RefundableDeposit));
    }

    #endregion
}
using AutoMapper;
using Moq;
using PujcovadloServer;
using PujcovadloServer.Authentication;
using PujcovadloServer.Business.Entities;
using PujcovadloServer.Business.Enums;
using PujcovadloServer.Business.Exceptions;
using PujcovadloServer.Business.Services;
using PujcovadloServer.Business.Services.Interfaces;
using PujcovadloServer.Business.States.Loan;
using PujcovadloServer.Requests;

namespace Tests.Business.Facades;

public class ReturnProtocolFacadeTest
{
    private ReturnProtocolFacade _returnProtocolFacade;
    private Mock<ImageFacade> _imageFacade;
    private Mock<LoanService> _loanService;
    private Mock<ReturnProtocolService> _returnProtocolService;
    private Mock<IAuthenticateService> _authenticateService;
    private Mock<IMapper> _mapper;
    private Mock<PujcovadloServerConfiguration> _configuration;

    private ApplicationUser _user;
    private ApplicationUser _owner;
    private Loan _loan;

    [SetUp]
    public void Setup()
    {
        _imageFacade = new Mock<ImageFacade>(null, null, null, null);
        _loanService = new Mock<LoanService>(null, null);
        _returnProtocolService = new Mock<ReturnProtocolService>(null);
        _authenticateService = new Mock<IAuthenticateService>();
        _mapper = new Mock<IMapper>();
        _configuration = new Mock<PujcovadloServerConfiguration>(null);

        _returnProtocolFacade = new ReturnProtocolFacade(_imageFacade.Object, _returnProtocolService.Object,
            _loanService.Object, _mapper.Object, _configuration.Object);

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

    #region GetReturnProtocol

    [Test]
    public void GetReturnProtocol_ReturnProtocolNotFound_ThrowsException()
    {
        // Arrange
        _loan.ReturnProtocol = null;

        // Must throw EntityNotFoundException because the protocol was not found
        Assert.Throws<EntityNotFoundException>(() => _returnProtocolFacade.GetReturnProtocol(_loan));
    }

    [Test]
    public void GetReturnProtocol_ReturnProtocolFound_ReturnsProtocol()
    {
        // Arrange
        _loan.ReturnProtocol = new ReturnProtocol()
            { Id = 1, Loan = _loan, ReturnedRefundableDeposit = 2000, Description = "All OK" };

        var expectedProtocol = new ReturnProtocol()
            { Id = 1, Loan = _loan, ReturnedRefundableDeposit = 2000, Description = "All OK" };

        // Run the operation
        var result = _returnProtocolFacade.GetReturnProtocol(_loan);

        // Check that the protocol was returned
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(expectedProtocol.Id));
        Assert.That(result.Loan.Id, Is.EqualTo(_loan.Id));
        Assert.That(result.ReturnedRefundableDeposit, Is.EqualTo(expectedProtocol.ReturnedRefundableDeposit));
        Assert.That(result.Description, Is.EqualTo(expectedProtocol.Description));
        Assert.That(result.ConfirmedAt, Is.EqualTo(expectedProtocol.ConfirmedAt));
    }

    #endregion

    #region CreateReturnProtocol

    [Test]
    public async Task CreateReturnProtocol_LoanStatusIsNotActiveOrReturnDenied_ThrowsException()
    {
        var disallowedStatuses = new List<LoanStatus>()
        {
            LoanStatus.Inquired,
            LoanStatus.Denied,
            LoanStatus.Accepted,
            LoanStatus.Cancelled,
            LoanStatus.PreparedForPickup,
            LoanStatus.PickupDenied,
            LoanStatus.PreparedForReturn,
            LoanStatus.ReturnDenied,
            LoanStatus.Returned
        };

        // Arrange
        var request = new ReturnProtocolRequest { Description = "All Ok", ReturnedRefundableDeposit = 2000 };

        var returnProtocol = new ReturnProtocol
        {
            Description = request.Description, ReturnedRefundableDeposit = request.ReturnedRefundableDeposit,
        };

        // Mock the state
        var state = new Mock<ILoanState>();
        state.Setup(x => x.CanCreateReturnProtocol(_loan)).Returns(false);
        _loanService.Setup(x => x.GetState(_loan)).Returns(state.Object);

        // Mock the mapper
        _mapper.Setup(x => x.Map<ReturnProtocol>(request)).Returns(returnProtocol);

        // Check that each disallowed status throws an exception
        foreach (var status in disallowedStatuses)
        {
            _loan.Status = status;
            _loan.ReturnProtocol = null;

            // Run the operation
            Assert.ThrowsAsync<OperationNotAllowedException>(() =>
                _returnProtocolFacade.CreateReturnProtocol(_loan, request));
        }
    }

    [Test]
    public void CreateReturnProtocol_LoanStatusActive_Success()
    {
        _loan.Status = LoanStatus.Active;
        _loan.ReturnProtocol = null;

        // Arrange
        var request = new ReturnProtocolRequest() { Description = "All Ok", ReturnedRefundableDeposit = 2000 };

        var returnProtocol = new ReturnProtocol
        {
            Description = request.Description, ReturnedRefundableDeposit = request.ReturnedRefundableDeposit,
        };

        // Mock state 
        var state = new Mock<ILoanState>();
        state.Setup(x => x.CanCreateReturnProtocol(_loan)).Returns(true);
        _loanService.Setup(x => x.GetState(_loan)).Returns(state.Object);

        // Mock the mapper
        _mapper.Setup(x => x.Map<ReturnProtocol>(request)).Returns(returnProtocol);

        // Run the operation
        Assert.DoesNotThrowAsync(() =>
            _returnProtocolFacade.CreateReturnProtocol(_loan, request));
    }

    [Test]
    public void CreateReturnProtocol_ReturnProtocolAlreadyExists_ThrowsException()
    {
        // Arrange
        var request = new ReturnProtocolRequest() { Description = "All Ok", ReturnedRefundableDeposit = 2000 };
        _loan.Status = LoanStatus.Active;
        _loan.ReturnProtocol = new ReturnProtocol() { Id = 1, Loan = _loan };

        // Mock state
        var state = new Mock<ILoanState>();
        state.Setup(x => x.CanCreateReturnProtocol(_loan)).Returns(true);
        _loanService.Setup(x => x.GetState(_loan)).Returns(state.Object);

        // Run the operation
        Assert.ThrowsAsync<OperationNotAllowedException>(async () =>
            await _returnProtocolFacade.CreateReturnProtocol(_loan, request));
    }

    [Test]
    public async Task CreateReturnProtocol_NoReturnProtocolExistsYet_Success()
    {
        // Arrange
        var request = new ReturnProtocolRequest() { Description = "All Ok", ReturnedRefundableDeposit = 2000 };
        _loan.Status = LoanStatus.Active;
        _loan.ReturnProtocol = null;

        // Arrange the mock
        _mapper.Setup(x => x.Map<ReturnProtocol>(request)).Returns(new ReturnProtocol()
        {
            Description = request.Description,
            ReturnedRefundableDeposit = request.ReturnedRefundableDeposit
        });

        // mock state
        var state = new Mock<ILoanState>();
        state.Setup(x => x.CanCreateReturnProtocol(_loan)).Returns(true);
        _loanService.Setup(x => x.GetState(_loan)).Returns(state.Object);

        // Run the operation
        var result = await _returnProtocolFacade.CreateReturnProtocol(_loan, request);

        // Check that the protocol was created
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Loan.Id, Is.EqualTo(_loan.Id));
        Assert.That(result.Description, Is.EqualTo(request.Description));
        Assert.That(result.ReturnedRefundableDeposit, Is.EqualTo(request.ReturnedRefundableDeposit));

        // Check that the protocol was created
        _returnProtocolService.Verify(x => x.Create(result), Times.Once);
    }

    #endregion

    #region UpdateReturnProtocol

    [Test]
    public async Task UpdateReturnProtocol_LoanStatusIsNotAccepted_ThrowsException()
    {
        // Arrange
        _loan.ReturnProtocol = new ReturnProtocol() { Id = 1, Loan = _loan };
        var request = new ReturnProtocolRequest() { Description = "All Ok", ReturnedRefundableDeposit = 2000 };

        var disallowedStatuses = new List<LoanStatus>()
        {
            LoanStatus.Inquired,
            LoanStatus.Denied,
            LoanStatus.Accepted,
            LoanStatus.Cancelled,
            LoanStatus.PreparedForPickup,
            LoanStatus.PickupDenied,
            LoanStatus.PreparedForReturn,
            LoanStatus.Returned
        };

        // Mock state
        var state = new Mock<ILoanState>();
        state.Setup(x => x.CanUpdateReturnProtocol(_loan)).Returns(false);
        _loanService.Setup(x => x.GetState(_loan)).Returns(state.Object);

        // Check that each disallowed status throws an exception
        foreach (var status in disallowedStatuses)
        {
            _loan.Status = status;

            // Run the operation
            Assert.ThrowsAsync<OperationNotAllowedException>(() =>
                _returnProtocolFacade.UpdateReturnProtocol(_loan.ReturnProtocol, request));
        }
    }

    [Test]
    public async Task UpdateReturnProtocol_ReturnProtocolExists_Success()
    {
        // Arrange statuses that allow the protocol to be updated
        var statuses = new List<LoanStatus>() { LoanStatus.Active, LoanStatus.ReturnDenied, };

        // Arrange
        var request = new ReturnProtocolRequest() { Description = "All Ok", ReturnedRefundableDeposit = 2000 };
        _loan.ReturnProtocol = new ReturnProtocol() { Id = 1, Loan = _loan };

        // Mock state
        var state = new Mock<ILoanState>();
        state.Setup(x => x.CanUpdateReturnProtocol(_loan)).Returns(true);
        _loanService.Setup(x => x.GetState(_loan)).Returns(state.Object);

        // Arrange the mock
        _mapper.Setup(x => x.Map<ReturnProtocol>(request)).Returns(new ReturnProtocol()
        {
            Description = request.Description,
            ReturnedRefundableDeposit = request.ReturnedRefundableDeposit
        });


        foreach (var status in statuses)
        {
            _loan.Status = status;

            // Run the operation
            await _returnProtocolFacade.UpdateReturnProtocol(_loan.ReturnProtocol, request);

            // Check that the protocol was created
            Assert.That(_loan.ReturnProtocol, Is.Not.Null);
            Assert.That(_loan.ReturnProtocol.Loan.Id, Is.EqualTo(_loan.ReturnProtocol.Id));
            Assert.That(_loan.ReturnProtocol.Description, Is.EqualTo(request.Description));
            Assert.That(_loan.ReturnProtocol.ReturnedRefundableDeposit, Is.EqualTo(request.ReturnedRefundableDeposit));
        }

        // Check that the protocol was updated exactly statuses.Count times
        _returnProtocolService.Verify(x => x.Update(_loan.ReturnProtocol), Times.Exactly(statuses.Count));
    }

    #endregion
}
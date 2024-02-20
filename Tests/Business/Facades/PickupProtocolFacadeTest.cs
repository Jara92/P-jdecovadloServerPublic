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

public class PickupProtocolFacadeTest
{
    private PickupProtocolFacade _pickupProtocolFacade;
    private Mock<ImageFacade> _imageFacade;
    private Mock<LoanService> _loanService;
    private Mock<PickupProtocolService> _pickupProtocolService;
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
        _pickupProtocolService = new Mock<PickupProtocolService>(null);
        _authenticateService = new Mock<IAuthenticateService>();
        _mapper = new Mock<IMapper>();
        _configuration = new Mock<PujcovadloServerConfiguration>(null);

        _pickupProtocolFacade = new PickupProtocolFacade(_imageFacade.Object, _pickupProtocolService.Object,
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

    #region GetPickupProtocol

    [Test]
    public void GetPickupProtocol_PickupProtocolNotFound_ThrowsException()
    {
        // Arrange
        _loan.PickupProtocol = null;

        // Must throw EntityNotFoundException because the protocol was not found
        Assert.Throws<EntityNotFoundException>(() => _pickupProtocolFacade.GetPickupProtocol(_loan));
    }

    [Test]
    public void GetPickupProtocol_PickupProtocolFound_ReturnsProtocol()
    {
        // Arrange
        _loan.PickupProtocol = new PickupProtocol()
            { Id = 1, Loan = _loan, AcceptedRefundableDeposit = 2000, Description = "All OK" };

        var expectedProtocol = new PickupProtocol()
            { Id = 1, Loan = _loan, AcceptedRefundableDeposit = 2000, Description = "All OK" };

        // Run the operation
        var result = _pickupProtocolFacade.GetPickupProtocol(_loan);

        // Check that the protocol was returned
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(expectedProtocol.Id));
        Assert.That(result.Loan.Id, Is.EqualTo(_loan.Id));
        Assert.That(result.AcceptedRefundableDeposit, Is.EqualTo(expectedProtocol.AcceptedRefundableDeposit));
        Assert.That(result.Description, Is.EqualTo(expectedProtocol.Description));
        Assert.That(result.ConfirmedAt, Is.EqualTo(expectedProtocol.ConfirmedAt));
    }

    #endregion

    #region CreatePickupProtocol

    [Test]
    public async Task CreatePickupProtocol_LoanStatusIsNotAccepted_ThrowsException()
    {
        var disallowedStatuses = new List<LoanStatus>()
        {
            LoanStatus.Inquired,
            LoanStatus.Denied,
            LoanStatus.Cancelled,
            LoanStatus.PreparedForPickup,
            LoanStatus.PickupDenied,
            LoanStatus.Active,
            LoanStatus.PreparedForReturn,
            LoanStatus.ReturnDenied,
            LoanStatus.Returned
        };

        // Mock state
        var state = new Mock<ILoanState>();
        state.Setup(x => x.CanCreatePickupProtocol(_loan)).Returns(false);
        _loanService.Setup(x => x.GetState(_loan)).Returns(state.Object);

        // Arrange
        var request = new PickupProtocolRequest() { Description = "All Ok", AcceptedRefundableDeposit = 2000 };

        // Check that each disallowed status throws an exception
        foreach (var status in disallowedStatuses)
        {
            _loan.Status = status;

            // Run the operation
            Assert.ThrowsAsync<OperationNotAllowedException>(() =>
                _pickupProtocolFacade.CreatePickupProtocol(_loan, request));
        }
    }

    [Test]
    public void CreatePickupProtocol_PickupProtocolAlreadyExists_ThrowsException()
    {
        // Arrange
        var request = new PickupProtocolRequest() { Description = "All Ok", AcceptedRefundableDeposit = 2000 };
        _loan.Status = LoanStatus.Accepted;
        _loan.PickupProtocol = new PickupProtocol() { Id = 1, Loan = _loan };

        // Mock state
        var state = new Mock<ILoanState>();
        state.Setup(x => x.CanCreatePickupProtocol(_loan)).Returns(true);
        _loanService.Setup(x => x.GetState(_loan)).Returns(state.Object);

        // Run the operation
        Assert.ThrowsAsync<OperationNotAllowedException>(async () =>
            await _pickupProtocolFacade.CreatePickupProtocol(_loan, request));
    }

    [Test]
    public async Task CreatePickupProtocol_NoPickupProtocolExistsYet_Success()
    {
        // Arrange
        var request = new PickupProtocolRequest() { Description = "All Ok", AcceptedRefundableDeposit = 2000 };
        _loan.Status = LoanStatus.Accepted;
        _loan.PickupProtocol = null;

        // Mock state
        var state = new Mock<ILoanState>();
        state.Setup(x => x.CanCreatePickupProtocol(_loan)).Returns(true);
        _loanService.Setup(x => x.GetState(_loan)).Returns(state.Object);

        // Arrange the mock
        _mapper.Setup(x => x.Map<PickupProtocol>(request)).Returns(new PickupProtocol()
        {
            Description = request.Description,
            AcceptedRefundableDeposit = request.AcceptedRefundableDeposit
        });

        // Run the operation
        var result = await _pickupProtocolFacade.CreatePickupProtocol(_loan, request);

        // Check that the protocol was created
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Loan.Id, Is.EqualTo(_loan.Id));
        Assert.That(result.Description, Is.EqualTo(request.Description));
        Assert.That(result.AcceptedRefundableDeposit, Is.EqualTo(request.AcceptedRefundableDeposit));

        // Check that the protocol was created
        _pickupProtocolService.Verify(x => x.Create(result), Times.Once);
    }

    #endregion

    #region UpdatePickupProtocol

    [Test]
    public async Task UpdatePickupProtocol_LoanStatusIsNotAccepted_ThrowsException()
    {
        // Arrange
        _loan.PickupProtocol = new PickupProtocol() { Id = 1, Loan = _loan };
        var request = new PickupProtocolRequest() { Description = "All Ok", AcceptedRefundableDeposit = 2000 };

        var disallowedStatuses = new List<LoanStatus>()
        {
            LoanStatus.Inquired,
            LoanStatus.Denied,
            LoanStatus.Cancelled,
            LoanStatus.PreparedForPickup,
            LoanStatus.Active,
            LoanStatus.PreparedForReturn,
            LoanStatus.ReturnDenied,
            LoanStatus.Returned
        };

        // Mock state
        var state = new Mock<ILoanState>();
        state.Setup(x => x.CanUpdatePickupProtocol(_loan)).Returns(false);
        _loanService.Setup(x => x.GetState(_loan)).Returns(state.Object);

        // Check that each disallowed status throws an exception
        foreach (var status in disallowedStatuses)
        {
            _loan.Status = status;

            // Run the operation
            Assert.ThrowsAsync<OperationNotAllowedException>(() =>
                _pickupProtocolFacade.UpdatePickupProtocol(_loan.PickupProtocol, request));
        }
    }

    [Test]
    public async Task UpdatePickupProtocol_PickupProtocolExists_Success()
    {
        // Arrange statuses that allow the protocol to be updated
        var statuses = new List<LoanStatus>() { LoanStatus.Accepted, LoanStatus.PickupDenied };

        // Arrange
        var request = new PickupProtocolRequest() { Description = "All Ok", AcceptedRefundableDeposit = 2000 };
        _loan.PickupProtocol = new PickupProtocol() { Id = 1, Loan = _loan };

        // Mock state
        var state = new Mock<ILoanState>();
        state.Setup(x => x.CanUpdatePickupProtocol(_loan)).Returns(true);
        _loanService.Setup(x => x.GetState(_loan)).Returns(state.Object);

        // Arrange the mock
        _mapper.Setup(x => x.Map<PickupProtocol>(request)).Returns(new PickupProtocol()
        {
            Description = request.Description,
            AcceptedRefundableDeposit = request.AcceptedRefundableDeposit
        });


        foreach (var status in statuses)
        {
            _loan.Status = status;

            // Run the operation
            await _pickupProtocolFacade.UpdatePickupProtocol(_loan.PickupProtocol, request);

            // Check that the protocol was created
            Assert.That(_loan.PickupProtocol, Is.Not.Null);
            Assert.That(_loan.PickupProtocol.Loan.Id, Is.EqualTo(_loan.PickupProtocol.Id));
            Assert.That(_loan.PickupProtocol.Description, Is.EqualTo(request.Description));
            Assert.That(_loan.PickupProtocol.AcceptedRefundableDeposit, Is.EqualTo(request.AcceptedRefundableDeposit));
        }

        // Check that the protocol was updated exactly statuses.Count times
        _pickupProtocolService.Verify(x => x.Update(_loan.PickupProtocol), Times.Exactly(statuses.Count));
    }

    #endregion
}
using AutoMapper;
using Moq;
using PujcovadloServer;
using PujcovadloServer.Authentication;
using PujcovadloServer.Business.Entities;
using PujcovadloServer.Business.Enums;
using PujcovadloServer.Business.Exceptions;
using PujcovadloServer.Business.Services;
using PujcovadloServer.Business.Services.Interfaces;
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
            _mapper.Object, _configuration.Object);

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

        // Run the operation
        Assert.ThrowsAsync<ActionNotAllowedException>(async () =>
            await _pickupProtocolFacade.CreatePickupProtocol(_loan, request));
    }

    [Test]
    public async Task CreatePickupProtocol_NoPickupProtocolExistsYet_Success()
    {
        // Arrange
        var request = new PickupProtocolRequest() { Description = "All Ok", AcceptedRefundableDeposit = 2000 };
        _loan.Status = LoanStatus.Accepted;
        _loan.PickupProtocol = null;

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
}
using AutoMapper;
using Moq;
using PujcovadloServer;
using PujcovadloServer.Authentication;
using PujcovadloServer.Business.Entities;
using PujcovadloServer.Business.Exceptions;
using PujcovadloServer.Business.Services;
using PujcovadloServer.Business.Services.Interfaces;
using PujcovadloServer.Business.States.Loan;
using PujcovadloServer.Requests;

namespace Tests.Business.Facades;

public class ReviewFacadeTest
{
    private ReviewFacade _reviewFacade;
    private Mock<ImageFacade> _imageFacade;
    private Mock<LoanService> _loanService;
    private Mock<ReviewService> _reviewService;
    private Mock<IAuthenticateService> _authenticateService;
    private Mock<IMapper> _mapper;
    private Mock<PujcovadloServerConfiguration> _configuration;

    private ApplicationUser _user;
    private ApplicationUser _owner;
    private ApplicationUser _tenant;
    private Loan _loan;

    [SetUp]
    public void Setup()
    {
        _imageFacade = new Mock<ImageFacade>(null, null, null, null);
        _loanService = new Mock<LoanService>(null, null);
        _reviewService = new Mock<ReviewService>(null);
        _authenticateService = new Mock<IAuthenticateService>();
        _mapper = new Mock<IMapper>();
        _configuration = new Mock<PujcovadloServerConfiguration>(null);

        _reviewFacade = new ReviewFacade(_reviewService.Object, _loanService.Object, _authenticateService.Object,
            _mapper.Object, _configuration.Object);

        _user = new ApplicationUser() { Id = "1" };
        _owner = new ApplicationUser() { Id = "2" };
        _tenant = new ApplicationUser() { Id = "3" };

        var from = DateTime.Now;

        _loan = new Loan()
        {
            Id = 1,
            Item = new Item() { Owner = _owner },
            Tenant = _tenant,
            From = from,
            To = from.AddDays(1),
            Days = 1,
            PricePerDay = 100,
            ExpectedPrice = 100,
            RefundableDeposit = 2000,
        };
    }

    #region GetReview

    [Test]
    public void GetReview_ReviewNotFound_ThrowsException()
    {
        var reviewId = 20;
        // Arrange
        _reviewService.Setup(x => x.Get(reviewId, true)).ReturnsAsync((Review)null);


        // Must throw EntityNotFoundException because the protocol was not found
        Assert.ThrowsAsync<EntityNotFoundException>(async () => await _reviewFacade.GetReview(reviewId));

        // Verify that the service was called
        _reviewService.Verify(x => x.Get(reviewId, true), Times.Once);
    }

    [Test]
    public async Task GetReview_ReviewFound_ReturnsProtocol()
    {
        var reviewId = 20;

        var review = new Review
        {
            Id = reviewId,
            Loan = _loan,
            Comment = "All OK",
            Rating = 5,
            Author = _owner
        };

        // Mock service
        _reviewService.Setup(x => x.Get(reviewId, true)).ReturnsAsync(review);

        // Run the operation
        var result = await _reviewFacade.GetReview(reviewId);

        // Check that the protocol was returned
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(reviewId));
    }

    #endregion

    #region CreateReview

    [Test]
    public async Task CreateReview_LoanStateDoesNotAllowToCreateAReview_ThrowsException()
    {
        // Mock existng review
        _reviewService.Setup(x => x.FindByLoanAndAuthor(_loan, _owner)).ReturnsAsync((Review)null);

        // Mock state
        var state = new Mock<ILoanState>();
        state.Setup(x => x.CanCreateReview(_loan)).Returns(false);
        _loanService.Setup(x => x.GetState(_loan)).Returns(state.Object);

        // Arrange
        var request = new ReviewRequest() { Comment = "All ok", Rating = 5 };

        // Run the operation
        Assert.ThrowsAsync<OperationNotAllowedException>(() =>
            _reviewFacade.CreateReview(_loan, request));
    }

    [Test]
    public void CreateReview_UserIsOwnerAndHisReviewAlreadyExists_ThrowsException()
    {
        // Arrange
        var request = new ReviewRequest() { Comment = "All Ok", Rating = 5 };

        var existingReview = new Review { Id = 1, Author = _owner, Loan = _loan, Comment = "OK", Rating = 5 };

        // Mock state
        var state = new Mock<ILoanState>();
        state.Setup(x => x.CanCreateReview(_loan)).Returns(true);
        _loanService.Setup(x => x.GetState(_loan)).Returns(state.Object);

        // Mock user
        _authenticateService.Setup(x => x.GetCurrentUser()).ReturnsAsync(_owner);

        // Mock existing review
        _reviewService.Setup(x => x.FindByLoanAndAuthor(_loan, _owner)).ReturnsAsync(existingReview);

        // Run the operation
        Assert.ThrowsAsync<OperationNotAllowedException>(async () =>
            await _reviewFacade.CreateReview(_loan, request));

        // Verify the create method was not called
        _reviewService.Verify(x => x.Create(It.IsAny<Review>()), Times.Never);
    }

    [Test]
    public void CreateReview_UserIsTenantAndHisReviewAlreadyExists_ThrowsException()
    {
        // Arrange
        var request = new ReviewRequest() { Comment = "All Ok", Rating = 5 };

        var existingReview = new Review { Id = 1, Author = _tenant, Loan = _loan, Comment = "OK", Rating = 5 };

        // Mock state
        var state = new Mock<ILoanState>();
        state.Setup(x => x.CanCreateReview(_loan)).Returns(true);
        _loanService.Setup(x => x.GetState(_loan)).Returns(state.Object);

        // Mock user
        _authenticateService.Setup(x => x.GetCurrentUser()).ReturnsAsync(_tenant);

        // Mock existing review
        _reviewService.Setup(x => x.FindByLoanAndAuthor(_loan, _tenant)).ReturnsAsync(existingReview);

        // Run the operation
        Assert.ThrowsAsync<OperationNotAllowedException>(async () =>
            await _reviewFacade.CreateReview(_loan, request));

        // Verify the create method was not called
        _reviewService.Verify(x => x.Create(It.IsAny<Review>()), Times.Never);
    }

    [Test]
    public async Task CreateReview_UserIsTheOwnerNoReviewExistsYet_Success()
    {
        // Arrange
        var request = new ReviewRequest() { Comment = "All Ok", Rating = 5 };

        // Mock state
        var state = new Mock<ILoanState>();
        state.Setup(x => x.CanCreateReview(_loan)).Returns(true);
        _loanService.Setup(x => x.GetState(_loan)).Returns(state.Object);

        // Mock user
        _authenticateService.Setup(x => x.GetCurrentUser()).ReturnsAsync(_owner);

        // Arrange the mock
        _mapper.Setup(x => x.Map<Review>(request)).Returns(new Review()
        {
            Comment = request.Comment,
            Rating = request.Rating
        });

        // Run the operation
        var result = await _reviewFacade.CreateReview(_loan, request);

        // Check that the protocol was created
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Loan.Id, Is.EqualTo(_loan.Id));
        Assert.That(result.Author.Id, Is.EqualTo(_owner.Id));
        Assert.That(result.Comment, Is.EqualTo(request.Comment));
        Assert.That(result.Rating, Is.EqualTo(request.Rating));

        // Check that the review was created
        _reviewService.Verify(x => x.Create(result), Times.Once);
    }

    [Test]
    public async Task CreateReview_UserIsTheTenantNoReviewExistsYet_Success()
    {
        // Arrange
        var request = new ReviewRequest() { Comment = "All Ok", Rating = 5 };

        // Mock state
        var state = new Mock<ILoanState>();
        state.Setup(x => x.CanCreateReview(_loan)).Returns(true);
        _loanService.Setup(x => x.GetState(_loan)).Returns(state.Object);

        // Mock user
        _authenticateService.Setup(x => x.GetCurrentUser()).ReturnsAsync(_tenant);

        // Arrange the mock
        _mapper.Setup(x => x.Map<Review>(request)).Returns(new Review()
        {
            Comment = request.Comment,
            Rating = request.Rating
        });

        // Run the operation
        var result = await _reviewFacade.CreateReview(_loan, request);

        // Check that the protocol was created
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Loan.Id, Is.EqualTo(_loan.Id));
        Assert.That(result.Author.Id, Is.EqualTo(_tenant.Id));
        Assert.That(result.Comment, Is.EqualTo(request.Comment));
        Assert.That(result.Rating, Is.EqualTo(request.Rating));

        // Check that the review was created
        _reviewService.Verify(x => x.Create(result), Times.Once);
    }

    #endregion
}
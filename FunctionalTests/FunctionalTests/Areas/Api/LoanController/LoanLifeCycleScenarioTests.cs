using System.Net;
using FunctionalTests.Helpers;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using PujcovadloServer;
using PujcovadloServer.Business.Enums;
using PujcovadloServer.Data;
using PujcovadloServer.Requests;
using PujcovadloServer.Responses;
using Xunit;
using Xunit.Abstractions;
using Assert = NUnit.Framework.Assert;

namespace FunctionalTests.FunctionalTests.Areas.Api.LoanController;

public class LoanLifeCycleScenarioTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _application;
    private readonly HttpClient _client;
    private readonly ITestOutputHelper _output;
    private TestData _data;

    private readonly string _apiPath = "/api/loans/";

    public LoanLifeCycleScenarioTests(CustomWebApplicationFactory<Program> factory, ITestOutputHelper output)
    {
        _application = factory;
        _client = _application.CreateClient();
        _output = output;
        _data = new TestData();

        // Arrange
        using (var scope = _application.Services.CreateScope())
        {
            var scopedServices = scope.ServiceProvider;
            var db = scopedServices.GetRequiredService<PujcovadloServerContext>();
            Utilities.ReinitializeDbForTests(db, _data);
        }
    }

    [Fact]
    public async Task SimpleLoanScenarioWithReviews()
    {
        // tenant creates a new loan
        UserHelper.SetAuthorizationHeader(_client, UserHelper.TenantToken);
        var tenantsLoan = await CreateLoan();

        // owner reads the loan
        UserHelper.SetAuthorizationHeader(_client, UserHelper.OwnerToken);
        var ownersLoan = await GetLoan(tenantsLoan.Id);

        // owner accepts the loan
        ownersLoan = await UpdateStatus(ownersLoan.Id, LoanStatus.Accepted);

        // Tenant reads the loan
        UserHelper.SetAuthorizationHeader(_client, UserHelper.TenantToken);
        tenantsLoan = await GetLoan(ownersLoan.Id);
        Assert.That(tenantsLoan.Status, Is.EqualTo(LoanStatus.Accepted)); // Expects the loan to be accepted

        // Owner chooses the easier way for pickup - directly activates the loan
        UserHelper.SetAuthorizationHeader(_client, UserHelper.OwnerToken);
        ownersLoan = await UpdateStatus(ownersLoan.Id, LoanStatus.Active);

        // Tenant reads the loan
        UserHelper.SetAuthorizationHeader(_client, UserHelper.TenantToken);
        tenantsLoan = await GetLoan(ownersLoan.Id);
        Assert.That(tenantsLoan.Status, Is.EqualTo(LoanStatus.Active)); // Expects the loan to be active

        // Owner accepts return of the item
        UserHelper.SetAuthorizationHeader(_client, UserHelper.OwnerToken);
        ownersLoan = await UpdateStatus(ownersLoan.Id, LoanStatus.Returned);

        // Tenant reads the loan
        UserHelper.SetAuthorizationHeader(_client, UserHelper.TenantToken);
        tenantsLoan = await GetLoan(ownersLoan.Id);
        Assert.That(tenantsLoan.Status, Is.EqualTo(LoanStatus.Returned)); // Expects the loan to be returned

        // Owner reviews the loan
        UserHelper.SetAuthorizationHeader(_client, UserHelper.OwnerToken);
        var ownerReview = await CreateReview(ownersLoan.Id, "All was ok", 5);

        // Tenant reads the the review
        UserHelper.SetAuthorizationHeader(_client, UserHelper.TenantToken);
        var reviews = (await GetLoan(tenantsLoan.Id)).Reviews;
        Assert.That(reviews, Has.Count.EqualTo(1));
        Assert.That(reviews[0].Comment, Is.EqualTo(ownerReview.Comment));
        Assert.That(reviews[0].Rating, Is.EqualTo(ownerReview.Rating));
        Assert.That(reviews[0].Author.Id, Is.EqualTo(tenantsLoan.Item.Owner.Id));

        // tenant adds a review
        UserHelper.SetAuthorizationHeader(_client, UserHelper.TenantToken);
        var tenantReview = await CreateReview(tenantsLoan.Id, "Not good. Item was very bad.", 2);

        // owner reads the review
        UserHelper.SetAuthorizationHeader(_client, UserHelper.OwnerToken);
        reviews = (await GetLoan(ownersLoan.Id)).Reviews;
        Assert.That(reviews, Has.Count.EqualTo(2));
        Assert.That(reviews[1].Comment, Is.EqualTo(tenantReview.Comment));
        Assert.That(reviews[1].Rating, Is.EqualTo(tenantReview.Rating));
        Assert.That(reviews[1].Author.Id, Is.EqualTo(tenantsLoan.Tenant.Id));
    }

    [Fact]
    public async Task LoanScenarioButLoanIsDenied()
    {
        // Tenant creates a new loan
        UserHelper.SetAuthorizationHeader(_client, UserHelper.TenantToken);
        var tenantsLoan = await CreateLoan();

        // Owner reads the loan
        UserHelper.SetAuthorizationHeader(_client, UserHelper.OwnerToken);
        var ownersLoan = await GetLoan(tenantsLoan.Id);

        // Owner denies the loan
        ownersLoan = await UpdateStatus(ownersLoan.Id, LoanStatus.Denied);

        // Owner reviews the loan
        var ownerReview = await CreateReview(ownersLoan.Id, "Tenant was not good", 1);

        // Tenant reviews the loan
        UserHelper.SetAuthorizationHeader(_client, UserHelper.TenantToken);
        var tenantReview = await CreateReview(tenantsLoan.Id, "Owner was not good", 1);
    }

    [Fact]
    public async Task LoanScenarioLoanAcceptedButCancelledByTenant()
    {
        // Tenant creates a new loan
        UserHelper.SetAuthorizationHeader(_client, UserHelper.TenantToken);
        var tenantsLoan = await CreateLoan();

        // Owner reads the loan
        UserHelper.SetAuthorizationHeader(_client, UserHelper.OwnerToken);
        var ownersLoan = await GetLoan(tenantsLoan.Id);

        // Owner denies the loan
        ownersLoan = await UpdateStatus(ownersLoan.Id, LoanStatus.Accepted);

        // Tenant accepts the loan
        UserHelper.SetAuthorizationHeader(_client, UserHelper.TenantToken);
        tenantsLoan = await UpdateStatus(tenantsLoan.Id, LoanStatus.Cancelled);

        // Owner reads the loan
        UserHelper.SetAuthorizationHeader(_client, UserHelper.OwnerToken);
        ownersLoan = await GetLoan(tenantsLoan.Id);

        // Owner is unhappy and writes a review
        UserHelper.SetAuthorizationHeader(_client, UserHelper.OwnerToken);
        var ownerReview = await CreateReview(ownersLoan.Id, "Tenant cancelled the loan", 1);
    }

    [Fact]
    public async Task LoanScenarioLoanAcceptedButCancelledByOwner()
    {
        // Tenant creates a new loan
        UserHelper.SetAuthorizationHeader(_client, UserHelper.TenantToken);
        var tenantsLoan = await CreateLoan();

        // Owner reads the loan
        UserHelper.SetAuthorizationHeader(_client, UserHelper.OwnerToken);
        var ownersLoan = await GetLoan(tenantsLoan.Id);

        // Owner accepts the loan
        ownersLoan = await UpdateStatus(ownersLoan.Id, LoanStatus.Accepted);

        // But after a while he realizes that the item is not available
        UserHelper.SetAuthorizationHeader(_client, UserHelper.OwnerToken);
        ownersLoan = await UpdateStatus(ownersLoan.Id, LoanStatus.Cancelled);

        // Tenant reads the loan
        UserHelper.SetAuthorizationHeader(_client, UserHelper.TenantToken);
        tenantsLoan = await GetLoan(ownersLoan.Id);

        // Tenant is unhappy and writes a review
        UserHelper.SetAuthorizationHeader(_client, UserHelper.TenantToken);
        var tenantReview = await CreateReview(tenantsLoan.Id, "Owner cancelled the loan", 1);
    }

    [Fact]
    public async Task LoanFullScenarioWithProtocols()
    {
        // tenant creates a new loan
        UserHelper.SetAuthorizationHeader(_client, UserHelper.TenantToken);
        var tenantsLoan = await CreateLoan();

        // owner reads the loan
        UserHelper.SetAuthorizationHeader(_client, UserHelper.OwnerToken);
        var ownersLoan = await GetLoan(tenantsLoan.Id);

        // owner accepts the loan
        ownersLoan = await UpdateStatus(ownersLoan.Id, LoanStatus.Accepted);

        // Tenant reads the loan
        UserHelper.SetAuthorizationHeader(_client, UserHelper.TenantToken);
        tenantsLoan = await GetLoan(ownersLoan.Id);
        Assert.That(tenantsLoan.Status, Is.EqualTo(LoanStatus.Accepted)); // Expects the loan to be accepted

        // Owner prepares the loan for pickup but forgets to set the protocol
        UserHelper.SetAuthorizationHeader(_client, UserHelper.OwnerToken);
        await UpdateStatus(ownersLoan.Id, LoanStatus.PreparedForPickup, HttpStatusCode.UnprocessableEntity);

        // Owner prepares the pickup protocol
        await UpdatePickupProtocol(ownersLoan.Id, "All ok, not broken", ownersLoan.RefundableDeposit);

        // Owner prepares the loan to be picked up by changing the status
        ownersLoan = await UpdateStatus(ownersLoan.Id, LoanStatus.PreparedForPickup);

        // tenant reads the loan
        UserHelper.SetAuthorizationHeader(_client, UserHelper.TenantToken);
        tenantsLoan = await GetLoan(ownersLoan.Id);

        // Tenant reads the protocol
        var tenantsPickupProtocol = await GetPickupProtocol(tenantsLoan.Id);
        Assert.That(tenantsPickupProtocol, Is.Not.Null);

        // Tenant does not agree with the protocol
        tenantsLoan = await UpdateStatus(tenantsLoan.Id, LoanStatus.PickupDenied);

        // Owner reads the loan
        UserHelper.SetAuthorizationHeader(_client, UserHelper.OwnerToken);
        ownersLoan = await GetLoan(ownersLoan.Id);

        // Owner reads the protocol and updates it
        var ownerPickupProtocol = await GetPickupProtocol(ownersLoan.Id);
        Assert.That(ownerPickupProtocol, Is.Not.Null);
        await UpdatePickupProtocol(ownersLoan.Id, "Item is a bit broken",
            ownerPickupProtocol.AcceptedRefundableDeposit);

        // Owner prepares the loan to be picked up by changing the status
        ownersLoan = await UpdateStatus(ownersLoan.Id, LoanStatus.PreparedForPickup);

        // Tenant reads the loan
        UserHelper.SetAuthorizationHeader(_client, UserHelper.TenantToken);
        tenantsLoan = await GetLoan(ownersLoan.Id);

        // Tenant reads the protocol
        tenantsPickupProtocol = await GetPickupProtocol(tenantsLoan.Id);

        // Tenant agrees with the protocol
        tenantsLoan = await UpdateStatus(tenantsLoan.Id, LoanStatus.Active);

        // The loan is being active now until the loan.To date is reached

        // Owner gets the loan
        UserHelper.SetAuthorizationHeader(_client, UserHelper.OwnerToken);
        ownersLoan = await GetLoan(ownersLoan.Id);

        // Owner prepares the loan for return but forgets to set the protocol
        await UpdateStatus(ownersLoan.Id, LoanStatus.PreparedForReturn, HttpStatusCode.UnprocessableEntity);

        // Owner builds a return protocol
        await UpdateReturnProtocol(ownersLoan.Id, "The item was destroyed", 0);

        // Owner prepares the loan to be returned by changing the status
        ownersLoan = await UpdateStatus(ownersLoan.Id, LoanStatus.PreparedForReturn);

        // Tenant reads the loan
        UserHelper.SetAuthorizationHeader(_client, UserHelper.TenantToken);
        tenantsLoan = await GetLoan(ownersLoan.Id);

        // Tenant reads the protocol
        var tenantsReturnProtocol = await GetReturnProtocol(tenantsLoan.Id);
        Assert.That(tenantsReturnProtocol, Is.Not.Null);

        // Tenant does not agree with the protocol
        tenantsLoan = await UpdateStatus(tenantsLoan.Id, LoanStatus.ReturnDenied);

        // Owner reads the loan
        UserHelper.SetAuthorizationHeader(_client, UserHelper.OwnerToken);
        ownersLoan = await GetLoan(ownersLoan.Id);

        // Owner reads the protocol and updates it
        var ownerReturnProtocol = await GetReturnProtocol(ownersLoan.Id);
        Assert.That(ownerReturnProtocol, Is.Not.Null);
        await UpdateReturnProtocol(ownersLoan.Id, "The item was not destroyed",
            ownerPickupProtocol.AcceptedRefundableDeposit);

        // Owner prepares the loan to be returned by changing the status
        ownersLoan = await UpdateStatus(ownersLoan.Id, LoanStatus.PreparedForReturn);

        // Tenant reads the loan
        UserHelper.SetAuthorizationHeader(_client, UserHelper.TenantToken);
        tenantsLoan = await GetLoan(ownersLoan.Id);

        // Tenant reads the protocol
        tenantsReturnProtocol = await GetReturnProtocol(tenantsLoan.Id);

        // Tenant agrees with the protocol
        tenantsLoan = await UpdateStatus(tenantsLoan.Id, LoanStatus.Returned);
    }

    #region HelperMethods

    private async Task<LoanResponse> CreateLoan()
    {
        var loanStartDate = DateTime.Now.AddDays(1);

        // Define example item request which is valid
        var newLoanRequest = new LoanRequest()
        {
            ItemId = _data.Item1.Id,
            From = loanStartDate,
            To = loanStartDate.AddDays(2), // 2 days
            TenantNote = "Tenants note to the owner",
            Status = LoanStatus.Inquired
        };

        // Perform the action
        var response = await _client.PostAsJsonAsync(_apiPath, newLoanRequest);

        // Check status
        response.EnsureSuccessStatusCode();

        // get data
        var loan = await response.Content.ReadAsAsync<LoanResponse>();

        // Check data
        Assert.That(loan, Is.Not.Null);
        Assert.That(loan.Id, Is.GreaterThan(0));
        Assert.That(loan.Item.Id, Is.EqualTo(newLoanRequest.ItemId));
        Assert.That(loan.From, Is.EqualTo(newLoanRequest.From));
        Assert.That(loan.To, Is.EqualTo(newLoanRequest.To));
        Assert.That(loan.TenantNote, Is.EqualTo(newLoanRequest.TenantNote));
        Assert.That(loan.Status, Is.EqualTo(LoanStatus.Inquired));

        return loan;
    }

    private async Task<LoanResponse> GetLoan(int id)
    {
        var response = await _client.GetAsync($"{_apiPath}{id}");

        // Check status
        response.EnsureSuccessStatusCode();

        // get data
        var loan = await response.Content.ReadAsAsync<LoanResponse>();
        return loan;
    }

    private async Task<LoanResponse> UpdateStatus(int loadId, LoanStatus newStatus,
        HttpStatusCode expectedStatus = HttpStatusCode.OK)
    {
        var request = new LoanUpdateRequest()
        {
            Id = loadId,
            Status = newStatus
        };

        // Perform the action
        var response = await _client.PutAsJsonAsync($"{_apiPath}{loadId}", request);

        // Check status
        Assert.That(response.StatusCode, Is.EqualTo(expectedStatus));

        // get data
        var loan = await response.Content.ReadAsAsync<LoanResponse>();

        // Check new data only if the http status was successful
        if (response.IsSuccessStatusCode)
        {
            Assert.That(loan.Status, Is.EqualTo(newStatus));
        }

        return loan;
    }

    private async Task<ReviewResponse> CreateReview(int loanId, string comment, float rating)
    {
        var request = new ReviewRequest
        {
            Comment = comment,
            Rating = rating
        };

        // Perform the action
        var response = await _client.PostAsJsonAsync($"{_apiPath}{loanId}/reviews", request);

        // Check status
        response.EnsureSuccessStatusCode();

        // get data
        var review = await response.Content.ReadAsAsync<ReviewResponse>();
        Assert.That(review.Comment, Is.EqualTo(comment));
        Assert.That(review.Rating, Is.EqualTo(rating));

        return review;
    }

    private async Task UpdatePickupProtocol(int loanId, string description, float? acceptedDeposit)
    {
        var request = new PickupProtocolRequest
        {
            Description = description,
            AcceptedRefundableDeposit = acceptedDeposit
        };

        // Perform the action
        var response = await _client.PutAsJsonAsync($"{_apiPath}{loanId}/pickup-protocol", request);

        // Check status
        response.EnsureSuccessStatusCode();
    }

    private async Task<PickupProtocolResponse> GetPickupProtocol(int loanId)
    {
        // Perform the action
        var response = await _client.GetAsync($"{_apiPath}{loanId}/pickup-protocol");

        // Check status
        response.EnsureSuccessStatusCode();

        // get data
        var protocol = await response.Content.ReadAsAsync<PickupProtocolResponse>();

        return protocol;
    }

    private async Task UpdateReturnProtocol(int loanId, string description, float? returnedDeposit)
    {
        var request = new ReturnProtocolRequest()
        {
            Description = description,
            ReturnedRefundableDeposit = returnedDeposit
        };

        // Perform the action
        var response = await _client.PutAsJsonAsync($"{_apiPath}{loanId}/return-protocol", request);

        // Check status
        response.EnsureSuccessStatusCode();
    }

    private async Task<ReturnProtocolResponse> GetReturnProtocol(int loanId)
    {
        // Perform the action
        var response = await _client.GetAsync($"{_apiPath}{loanId}/return-protocol");

        // Check status
        response.EnsureSuccessStatusCode();

        // get data
        var protocol = await response.Content.ReadAsAsync<ReturnProtocolResponse>();

        return protocol;
    }

    #endregion
}
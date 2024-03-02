using System.Net;
using IntegrationTests.Helpers;
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

namespace IntegrationTests.IntegrationTests.Areas.Api.Controllers.LoanController;

public class ActiveLoanActionsTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _application;
    private readonly HttpClient _client;
    private readonly ITestOutputHelper _output;
    private TestData _data;

    private LoanUpdateRequest _loanUpdateRequest;

    private readonly string _apiPath = "/api/loans/";

    public ActiveLoanActionsTests(CustomWebApplicationFactory<Program> factory, ITestOutputHelper output)
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

        // Define example update request
        _loanUpdateRequest = new LoanUpdateRequest()
        {
            Id = _data.Loan1.Id,
            Status = LoanStatus.Accepted
        };
    }

    [Fact]
    public async Task PrepareForReturn_OwnerPreparesTheLoanForReturnButNoReturnProtocolSet_UnprocessableEntity()
    {
        UserHelper.SetAuthorizationHeader(_client, UserHelper.OwnerToken);

        // Set return protocol to null
        _data.LoanActive.ReturnProtocol = null;

        // Set loan id and new status
        _loanUpdateRequest.Id = _data.LoanActive.Id;
        _loanUpdateRequest.Status = LoanStatus.PreparedForReturn;

        // Perform the action
        var response = await _client.PutAsJsonAsync($"{_apiPath}{_loanUpdateRequest.Id}", _loanUpdateRequest);

        // Check http status
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.UnprocessableEntity));
    }

    [Fact]
    public async Task PrepareForReturn_OwnerPreparesTheLoanForReturnButThereWasNoPickupProtocol_UnprocessableEntity()
    {
        UserHelper.SetAuthorizationHeader(_client, UserHelper.OwnerToken);

        // make sure pickup protocol is null
        _data.LoanActive.PickupProtocol = null;

        // Set loan id and new status
        _loanUpdateRequest.Id = _data.LoanActive.Id;
        _loanUpdateRequest.Status = LoanStatus.PreparedForReturn;

        // Perform the action
        var response = await _client.PutAsJsonAsync($"{_apiPath}{_loanUpdateRequest.Id}", _loanUpdateRequest);

        // Check http status
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.UnprocessableEntity));
    }

    [Fact]
    public async Task PrepareForReturn_OwnerPreparesTheLoanForReturn_Ok()
    {
        UserHelper.SetAuthorizationHeader(_client, UserHelper.OwnerToken);

        // Set loan id and new status
        _loanUpdateRequest.Id = _data.LoanActiveHasBothProtocols.Id;
        _loanUpdateRequest.Status = LoanStatus.PreparedForReturn;

        // Perform the action
        var response = await _client.PutAsJsonAsync($"{_apiPath}{_loanUpdateRequest.Id}", _loanUpdateRequest);

        // Check http status
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        // Check if the loan status was updated
        var updatedLoan = await response.Content.ReadAsAsync<LoanResponse>();
        Assert.That(updatedLoan.Status, Is.EqualTo(_loanUpdateRequest.Status));
    }

    [Fact]
    public async Task Return_OwnerReturnsTheLoan_Ok()
    {
        UserHelper.SetAuthorizationHeader(_client, UserHelper.OwnerToken);

        // Make sure the protocols are not set
        _data.LoanActive.PickupProtocol = null;
        _data.LoanActive.ReturnProtocol = null;

        // Set loan id and new status
        _loanUpdateRequest.Id = _data.LoanActive.Id;
        _loanUpdateRequest.Status = LoanStatus.Returned;

        // Perform the action
        var response = await _client.PutAsJsonAsync($"{_apiPath}{_loanUpdateRequest.Id}", _loanUpdateRequest);

        // Check http status
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        // Check if the loan status was updated
        var updatedLoan = await response.Content.ReadAsAsync<LoanResponse>();
        Assert.That(updatedLoan.Status, Is.EqualTo(_loanUpdateRequest.Status));
    }

    [Fact]
    public async Task Return_OwnerReturnsTheLoanButItHasPickupProtocolSet_UnprocessableEntity()
    {
        UserHelper.SetAuthorizationHeader(_client, UserHelper.OwnerToken);

        // Set loan id and new status
        _loanUpdateRequest.Id = _data.LoanActiveHasBothProtocols.Id;
        _loanUpdateRequest.Status = LoanStatus.Returned;

        // Perform the action
        var response = await _client.PutAsJsonAsync($"{_apiPath}{_loanUpdateRequest.Id}", _loanUpdateRequest);

        // Should fail because loan has a pickup protocol set and it cannot be returned directly
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.UnprocessableEntity));
    }

    [Fact]
    public async Task DisallowedActions_OwnerTriesToChangeStatusToDisallowed_UnprocessableEntity()
    {
        UserHelper.SetAuthorizationHeader(_client, UserHelper.OwnerToken);

        // Set loan id and new status
        _loanUpdateRequest.Id = _data.LoanActiveHasBothProtocols.Id;

        var disallowedStatuses = new List<LoanStatus>
        {
            LoanStatus.Inquired,
            LoanStatus.Cancelled,
            LoanStatus.Accepted,
            LoanStatus.Denied,
            LoanStatus.PreparedForPickup,
            LoanStatus.PickupDenied,
            LoanStatus.ReturnDenied
        };

        foreach (var status in disallowedStatuses)
        {
            _loanUpdateRequest.Status = status;

            // Perform the action
            var response = await _client.PutAsJsonAsync($"{_apiPath}{_loanUpdateRequest.Id}", _loanUpdateRequest);

            // Check http status
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.UnprocessableEntity));
        }
    }

    [Fact]
    public async Task DisallowedActions_TenantTriesToChangeStatusToDisallowed_UnprocessableEntity()
    {
        UserHelper.SetAuthorizationHeader(_client, UserHelper.TenantToken);

        // Set loan id and new status
        _loanUpdateRequest.Id = _data.LoanActiveHasBothProtocols.Id;

        var disallowedStatuses = new List<LoanStatus>
        {
            LoanStatus.Inquired,
            LoanStatus.Cancelled,
            LoanStatus.Accepted,
            LoanStatus.Denied,
            LoanStatus.PreparedForPickup,
            LoanStatus.PickupDenied,
            LoanStatus.PreparedForReturn,
            LoanStatus.ReturnDenied,
            LoanStatus.Returned,
        };

        foreach (var status in disallowedStatuses)
        {
            _loanUpdateRequest.Status = status;

            // Perform the action
            var response = await _client.PutAsJsonAsync($"{_apiPath}{_loanUpdateRequest.Id}", _loanUpdateRequest);

            // Check http status
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.UnprocessableEntity));
        }
    }
}
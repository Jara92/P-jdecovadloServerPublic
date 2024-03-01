using System.Net;
using FunctionalTests.Helpers;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using PujcovadloServer;
using PujcovadloServer.Business.Enums;
using PujcovadloServer.Data;
using PujcovadloServer.Requests;
using Xunit;
using Xunit.Abstractions;
using Assert = NUnit.Framework.Assert;

namespace FunctionalTests.FunctionalTests.Areas.Api.LoanController;

public class CancelledLoanActionsTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _application;
    private readonly HttpClient _client;
    private readonly ITestOutputHelper _output;
    private TestData _data;

    private LoanUpdateRequest _loanUpdateRequest;

    private readonly string _apiPath = "/api/loans/";

    public CancelledLoanActionsTests(CustomWebApplicationFactory<Program> factory, ITestOutputHelper output)
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
    public async Task DisallowedActions_OwnerTriesToChangeStatusToDisallowed_UnprocessableEntity()
    {
        UserHelper.SetAuthorizationHeader(_client, UserHelper.OwnerToken);

        // Set loan id and new status
        _loanUpdateRequest.Id = _data.LoanCancelled.Id;

        var disallowedStatuses = new List<LoanStatus>
        {
            LoanStatus.Inquired,
            LoanStatus.Accepted,
            LoanStatus.Denied,
            LoanStatus.PreparedForPickup,
            LoanStatus.PickupDenied,
            LoanStatus.Active,
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

    [Fact]
    public async Task DisallowedActions_TenantTriesToChangeStatusToDisallowed_UnprocessableEntity()
    {
        UserHelper.SetAuthorizationHeader(_client, UserHelper.TenantToken);

        // Set loan id and new status
        _loanUpdateRequest.Id = _data.LoanCancelled.Id;

        var disallowedStatuses = new List<LoanStatus>
        {
            LoanStatus.Inquired,
            LoanStatus.Accepted,
            LoanStatus.Denied,
            LoanStatus.PreparedForPickup,
            LoanStatus.PickupDenied,
            LoanStatus.Active,
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
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

namespace FunctionalTests.FunctionalTests.Areas.Api.Loan;

public class UpdateTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _application;
    private readonly HttpClient _client;
    private readonly PujcovadloServerContext _db;
    private readonly ITestOutputHelper _output;
    private TestData _data;

    private readonly LoanUpdateRequest _loanUpdateRequest;

    private readonly string _apiPath = "/api/loans/";

    public UpdateTests(CustomWebApplicationFactory<Program> factory, ITestOutputHelper output)
    {
        _application = factory;
        _client = _application.CreateClient();
        _output = output;
        _data = new TestData();

        // Arrange
        using (var scope = _application.Services.CreateScope())
        {
            var scopedServices = scope.ServiceProvider;
            _db = scopedServices.GetRequiredService<PujcovadloServerContext>();

            Utilities.ReinitializeDbForTests(_db, _data);
        }

        var loanStartDate = DateTime.Now.AddDays(1);

        // Define example update request
        _loanUpdateRequest = new LoanUpdateRequest()
        {
            Id = _data.Loan1.Id,
            Status = LoanStatus.Accepted
        };
    }

    #region Authorization

    [Fact]
    public async Task Update_UnauthorizedUser_Unauthorized()
    {
        UserHelper.SetAuthorizationHeader(_client, UserHelper.UnauthorizedUserToken);

        // Perform the action
        var response = await _client.PutAsJsonAsync(_apiPath, _loanUpdateRequest);

        // Check http status
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Fact]
    public async Task Update_UserHasOnlyUserRole_Forbidden()
    {
        UserHelper.SetAuthorizationHeader(_client, UserHelper.UserToken);

        // Perform the action
        var response = await _client.PutAsJsonAsync(_apiPath + _loanUpdateRequest.Id, _loanUpdateRequest);

        // Check http status
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
    }

    [Fact]
    public async Task Update_UserHasOnlyOwnerRole_Forbidden()
    {
        // Owner2 is not the owner of the item
        UserHelper.SetAuthorizationHeader(_client, UserHelper.Owner2Token);

        // Perform the action
        var response = await _client.PutAsJsonAsync(_apiPath + _loanUpdateRequest.Id, _loanUpdateRequest);

        // Check http status
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
    }

    #endregion

    [Fact]
    public async Task Update_DifferentIdThanInRequest_BadRequest()
    {
        UserHelper.SetAuthorizationHeader(_client, UserHelper.OwnerToken);

        // Set loan id and new status
        _loanUpdateRequest.Id = _data.Loan1.Id;
        _loanUpdateRequest.Status = LoanStatus.Accepted;

        // Perform the action
        var response = await _client.PutAsJsonAsync($"{_apiPath}{_loanUpdateRequest.Id + 1}", _loanUpdateRequest);

        // Check http status
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Fact]
    public async Task Update_OwnerUpdatesButTheStateIsTheSame_Ok()
    {
        UserHelper.SetAuthorizationHeader(_client, UserHelper.OwnerToken);

        // Set loan id and new status
        _loanUpdateRequest.Id = _data.Loan1.Id;
        _loanUpdateRequest.Status = _data.Loan1.Status;

        // Perform the action
        var response = await _client.PutAsJsonAsync($"{_apiPath}{_loanUpdateRequest.Id}", _loanUpdateRequest);

        // Check http status
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Fact]
    public async Task Update_TenantUpdatesButTheStateIsTheSame_Ok()
    {
        UserHelper.SetAuthorizationHeader(_client, UserHelper.TenantToken);

        // Set loan id and new status
        _loanUpdateRequest.Id = _data.Loan1.Id;
        _loanUpdateRequest.Status = _data.Loan1.Status;

        // Perform the action
        var response = await _client.PutAsJsonAsync($"{_apiPath}{_loanUpdateRequest.Id}", _loanUpdateRequest);

        // Check http status
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Fact]
    public async Task Update_OwnerChangesTenantNote_Ok()
    {
        UserHelper.SetAuthorizationHeader(_client, UserHelper.OwnerToken);

        // Set loan id and new status
        _loanUpdateRequest.Id = _data.Loan1.Id;
        _loanUpdateRequest.Status = _data.Loan1.Status;
        _loanUpdateRequest.TenantNote = "New tenant note";

        // Perform the action
        var response = await _client.PutAsJsonAsync($"{_apiPath}{_loanUpdateRequest.Id}", _loanUpdateRequest);

        // Check http status
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        // Get response body
        var loanResponse = await response.Content.ReadAsAsync<LoanResponse>();

        // Tenant note should not be updated
        Assert.That(loanResponse.TenantNote, Is.EqualTo(_data.Loan1.TenantNote));
    }

    [Fact]
    public async Task Update_TenantChangesTenantNote_Ok()
    {
        UserHelper.SetAuthorizationHeader(_client, UserHelper.TenantToken);

        // Set loan id and new status
        _loanUpdateRequest.Id = _data.Loan1.Id;
        _loanUpdateRequest.Status = _data.Loan1.Status;
        _loanUpdateRequest.TenantNote = "New tenant note";

        // Perform the action
        var response = await _client.PutAsJsonAsync($"{_apiPath}{_loanUpdateRequest.Id}", _loanUpdateRequest);

        // Check http status
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        // Get response body
        var loanResponse = await response.Content.ReadAsAsync<LoanResponse>();

        // Tenant note should not be updated
        Assert.That(loanResponse.TenantNote, Is.EqualTo(_data.Loan1.TenantNote));
    }
}
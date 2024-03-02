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

public class CreateTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _application;
    private readonly HttpClient _client;
    private readonly PujcovadloServerContext _db;
    private readonly ITestOutputHelper _output;
    private TestData _data;

    private readonly LoanRequest _loanRequest;

    private readonly string _apiPath = "/api/loans/";

    public CreateTests(CustomWebApplicationFactory<Program> factory, ITestOutputHelper output)
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

        // Define example item request which is valid
        _loanRequest = new LoanRequest
        {
            ItemId = _data.Item1.Id,
            From = loanStartDate,
            To = loanStartDate.AddDays(2), // 2 days
            TenantNote = "Tenants note to the owner",
            Status = LoanStatus.Inquired
        };
    }

    #region Authorization

    [Fact]
    public async Task Create_UnauthorizedUser_Unauthorized()
    {
        UserHelper.SetAuthorizationHeader(_client, UserHelper.UnauthorizedUserToken);

        // Perform the action
        var response = await _client.PostAsJsonAsync(_apiPath, _loanRequest);

        // Check http status
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Fact]
    public async Task Create_UserHasOnlyUserRole_Forbidden()
    {
        UserHelper.SetAuthorizationHeader(_client, UserHelper.UserToken);

        // Perform the action
        var response = await _client.PostAsJsonAsync(_apiPath, _loanRequest);

        // Check http status
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
    }

    [Fact]
    public async Task Create_UserHasOnlyOwnerRole_Forbidden()
    {
        // Owner2 is not the owner of the item
        UserHelper.SetAuthorizationHeader(_client, UserHelper.Owner2Token);

        // Perform the action
        var response = await _client.PostAsJsonAsync(_apiPath, _loanRequest);

        // Check http status
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
    }

    #endregion

    [Fact]
    public async Task Create_ValidRequest_Created()
    {
        UserHelper.SetAuthorizationHeader(_client, UserHelper.TenantToken);

        // Perform the action
        var response = await _client.PostAsJsonAsync(_apiPath, _loanRequest);

        // Check http status
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));

        // Check data
        var loan = await response.Content.ReadAsAsync<LoanResponse>();
        Assert.That(loan, Is.Not.Null);
        Assert.That(loan.Id, Is.GreaterThan(0));
        Assert.That(loan.Item.Id, Is.EqualTo(_loanRequest.ItemId));
        Assert.That(loan.From, Is.EqualTo(_loanRequest.From));
        Assert.That(loan.TenantNote, Is.EqualTo(_loanRequest.TenantNote));
        Assert.That(loan.Tenant.Id, Is.EqualTo(UserHelper.TenantId));
        Assert.That(loan.Status, Is.EqualTo(LoanStatus.Inquired));
        Assert.That(loan.To, Is.EqualTo(_loanRequest.To));
        Assert.That(loan.Days, Is.EqualTo(2)); // 2 days are expected
        Assert.That(loan.ExpectedPrice, Is.EqualTo(loan.Days * _data.Item1.PricePerDay));
        Assert.That(loan.RefundableDeposit, Is.EqualTo(_data.Item1.RefundableDeposit));
    }

    [Fact]
    public async Task Create_ItemDoesNotExist_NotFound()
    {
        UserHelper.SetAuthorizationHeader(_client, UserHelper.TenantToken);

        // Set invalid item id
        _loanRequest.ItemId = 100;

        // Perform the action
        var response = await _client.PostAsJsonAsync(_apiPath, _loanRequest);

        // Check http status
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Fact]
    public async Task Create_CustomStatusSet_DefaultStatusSetInstead()
    {
        UserHelper.SetAuthorizationHeader(_client, UserHelper.TenantToken);

        // Set invalid item id
        _loanRequest.Status = LoanStatus.Active;

        // Perform the action
        var response = await _client.PostAsJsonAsync(_apiPath, _loanRequest);

        // Check http status
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));

        // Check data
        var loan = await response.Content.ReadAsAsync<LoanResponse>();
        Assert.That(loan, Is.Not.Null);
        Assert.That(loan.Status, Is.EqualTo(LoanStatus.Inquired)); // Default status is set
    }

    [Fact]
    public async Task Create_FromIsGreaterThanTo_BadRequest()
    {
        UserHelper.SetAuthorizationHeader(_client, UserHelper.TenantToken);

        // Set invalid dates
        _loanRequest.From = DateTime.Now.AddDays(2);
        _loanRequest.To = DateTime.Now.AddDays(1);

        // Perform the action
        var response = await _client.PostAsJsonAsync(_apiPath, _loanRequest);

        // Check http status
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Fact]
    public async Task Create_FromIsLessThanNow_BadRequest()
    {
        UserHelper.SetAuthorizationHeader(_client, UserHelper.TenantToken);

        // Set invalid dates
        _loanRequest.From = DateTime.Now.AddDays(-1);
        _loanRequest.To = DateTime.Now.AddDays(1);

        // Perform the action
        var response = await _client.PostAsJsonAsync(_apiPath, _loanRequest);

        // Check http status
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Fact]
    public async Task Create_FromIsSameAsTo_BadRequest()
    {
        UserHelper.SetAuthorizationHeader(_client, UserHelper.TenantToken);

        // Set invalid dates
        _loanRequest.To = _loanRequest.From;

        // Perform the action
        var response = await _client.PostAsJsonAsync(_apiPath, _loanRequest);

        // Check http status
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));

        // Check data
        var loan = await response.Content.ReadAsAsync<LoanResponse>();
        Assert.That(loan, Is.Not.Null);
        Assert.That(loan.Days, Is.EqualTo(1)); // 1 day is expected
    }

    [Fact]
    public async Task Create_UserIsTheOwnerOfTheItem_UnprocessableEntity()
    {
        UserHelper.SetAuthorizationHeader(_client, UserHelper.OwnerToken);

        // Perform the action
        var response = await _client.PostAsJsonAsync(_apiPath, _loanRequest);

        // Check http status - Owner cant borrow his own item
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.UnprocessableEntity));
    }

    [Fact]
    public async Task Create_ItemIsInApprovingStatus_Forbidden()
    {
        UserHelper.SetAuthorizationHeader(_client, UserHelper.TenantToken);

        _loanRequest.ItemId = _data.ItemApproving.Id;

        // Perform the action
        var response = await _client.PostAsJsonAsync(_apiPath, _loanRequest);

        // Check http status - nobody can borrow non public item
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
    }

    [Fact]
    public async Task Create_ItemIsInDeniedStatus_Forbidden()
    {
        UserHelper.SetAuthorizationHeader(_client, UserHelper.TenantToken);

        _loanRequest.ItemId = _data.ItemDenied.Id;

        // Perform the action
        var response = await _client.PostAsJsonAsync(_apiPath, _loanRequest);

        // Check http status - nobody can borrow non public item
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
    }

    [Fact]
    public async Task Create_ItemIsDeleted_Forbidden()
    {
        UserHelper.SetAuthorizationHeader(_client, UserHelper.TenantToken);

        _loanRequest.ItemId = _data.ItemDeleted.Id;

        // Perform the action
        var response = await _client.PostAsJsonAsync(_apiPath, _loanRequest);

        // Check http status - nobody can borrow non public item
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
    }
}
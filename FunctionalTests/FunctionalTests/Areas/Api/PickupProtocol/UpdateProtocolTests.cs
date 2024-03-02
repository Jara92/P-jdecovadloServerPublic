using System.Net;
using FunctionalTests.Helpers;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using PujcovadloServer;
using PujcovadloServer.Data;
using PujcovadloServer.Requests;
using PujcovadloServer.Responses;
using Xunit;
using Xunit.Abstractions;
using Assert = NUnit.Framework.Assert;

namespace FunctionalTests.FunctionalTests.Areas.Api.PickupProtocol;

public class UpdateProtocolTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _application;
    private readonly HttpClient _client;
    private readonly PujcovadloServerContext _db;
    private readonly ITestOutputHelper _output;
    private TestData _data;

    private readonly PickupProtocolRequest _protocolUpdateRequest;

    private readonly string _apiPath = "/api/loans/";

    public UpdateProtocolTests(CustomWebApplicationFactory<Program> factory, ITestOutputHelper output)
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
        _protocolUpdateRequest = new PickupProtocolRequest()
        {
            Description = "All was ok",
            AcceptedRefundableDeposit = 200
        };
    }

    #region Authorization

    [Fact]
    public async Task Update_UnauthorizedUser_Unauthorized()
    {
        UserHelper.SetAuthorizationHeader(_client, UserHelper.UnauthorizedUserToken);

        // Perform the action
        var response = await _client.PutAsJsonAsync($"{_apiPath}{_data.LoanAccepted.Id}/pickup-protocol",
            _protocolUpdateRequest);

        // Check http status
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Fact]
    public async Task Update_UserHasOnlyUserRole_Forbidden()
    {
        UserHelper.SetAuthorizationHeader(_client, UserHelper.UserToken);

        // Perform the action
        var response = await _client.PutAsJsonAsync($"{_apiPath}{_data.LoanAccepted.Id}/pickup-protocol",
            _protocolUpdateRequest);

        // Check http status
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
    }

    [Fact]
    public async Task Update_UserHasOnlyOwnerRole_Forbidden()
    {
        // Owner2 is not the owner of the item and he cannot update the protocol
        UserHelper.SetAuthorizationHeader(_client, UserHelper.Owner2Token);

        // Perform the action
        var response = await _client.PutAsJsonAsync($"{_apiPath}{_data.LoanAccepted.Id}/pickup-protocol",
            _protocolUpdateRequest);

        // Check http status
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
    }

    [Fact]
    public async Task Update_UserHasOnlyTenantRoleButNotTheTenant_Forbidden()
    {
        // Tenant2 is not the tenant of the item and he cannot update the protocol
        UserHelper.SetAuthorizationHeader(_client, UserHelper.Tenant2Token);

        // Perform the action
        var response = await _client.PutAsJsonAsync($"{_apiPath}{_data.LoanAccepted.Id}/pickup-protocol",
            _protocolUpdateRequest);

        // Check http status
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
    }

    [Fact]
    public async Task Update_UserIsTheTenant_Forbidden()
    {
        // Tenant is the tenant of the item but he cannot update the protocol
        UserHelper.SetAuthorizationHeader(_client, UserHelper.TenantToken);

        // Perform the action
        var response = await _client.PutAsJsonAsync($"{_apiPath}{_data.LoanAccepted.Id}/pickup-protocol",
            _protocolUpdateRequest);

        // Check http status
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
    }

    #endregion

    [Fact]
    public async Task Update_UserIsTheOwnerAndStatusIsAccepted_Ok()
    {
        UserHelper.SetAuthorizationHeader(_client, UserHelper.OwnerToken);

        // Perform the action
        var response = await _client.PutAsJsonAsync($"{_apiPath}{_data.LoanAccepted.Id}/pickup-protocol",
            _protocolUpdateRequest);

        // Check http status
        Assert.That(response.StatusCode,
            Is.EqualTo(HttpStatusCode.Created)); // Http create because the protocol did not exist

        // Update the protocol because of some mistake
        _protocolUpdateRequest.Description = "All was not ok (updated)";
        response = await _client.PutAsJsonAsync($"{_apiPath}{_data.LoanAccepted.Id}/pickup-protocol",
            _protocolUpdateRequest);

        // Check http status
        Assert.That(response.StatusCode,
            Is.EqualTo(HttpStatusCode.NoContent)); // HTTP no content because the protocol already existed

        // Add image
        var image1 = await AddImage(_data.LoanAccepted.Id, "FunctionalTests/data/images/img1.jpg");
        var image2 = await AddImage(_data.LoanAccepted.Id, "FunctionalTests/data/images/img2.jpg");

        // Get images
        var images = await GetImages(_data.LoanAccepted.Id, HttpStatusCode.OK);
        Assert.That(images._data.Count, Is.EqualTo(2));

        // Delete image
        await DeleteImage(_data.LoanAccepted.Id, image1.Id, HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Update_UserIsTheOwnerAndStatusIsPickupDenied_Ok()
    {
        UserHelper.SetAuthorizationHeader(_client, UserHelper.OwnerToken);

        // Perform the action
        var response = await _client.PutAsJsonAsync($"{_apiPath}{_data.LoanPickupDenied.Id}/pickup-protocol",
            _protocolUpdateRequest);

        // Check http status
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent)); // Protocol must have been create before

        // Add image
        var image1 = await AddImage(_data.LoanPickupDenied.Id, "FunctionalTests/data/images/img1.jpg");
        var image2 = await AddImage(_data.LoanPickupDenied.Id, "FunctionalTests/data/images/img2.jpg");

        // Get images
        var images = await GetImages(_data.LoanPickupDenied.Id, HttpStatusCode.OK);
        Assert.That(images._data.Count, Is.EqualTo(2));

        // Delete image
        await DeleteImage(_data.LoanPickupDenied.Id, image1.Id, HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Update_UserIsTheOwnerButStatusDoesNotAllowToUpdateTheProtocol_UnprocessableEntity()
    {
        UserHelper.SetAuthorizationHeader(_client, UserHelper.OwnerToken);

        var loansWhichStatusDoesNotAllowToUpdateTheProtocol = new List<PujcovadloServer.Business.Entities.Loan>
        {
            _data.LoanInquired,
            _data.LoanCancelled,
            _data.LoanDenied,
            _data.LoanPreparedForPickup,
            _data.LoanActive,
            _data.LoanPreparedForReturn,
            _data.LoanReturnDenied,
            _data.LoanReturned
        };

        foreach (var loan in loansWhichStatusDoesNotAllowToUpdateTheProtocol)
        {
            // Perform the action
            var response = await _client.PutAsJsonAsync($"{_apiPath}{loan.Id}/pickup-protocol", _protocolUpdateRequest);

            // Check http status
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.UnprocessableEntity));
        }
    }

    #region HelperMethods

    private async Task<ImageResponse> AddImage(int loanId, string path, string mimeType = "image/jpeg",
        HttpStatusCode expectedStatusCode = HttpStatusCode.Created)
    {
        var content = FileUploadHelper.CreateFileUploadForm(path, mimeType);

        // perform the action
        var response = await _client.PostAsync($"{_apiPath}{loanId}/pickup-protocol/images", content);

        // Check http status
        Assert.That(response.StatusCode, Is.EqualTo(expectedStatusCode));

        // Get the response
        var responseImage = await response.Content.ReadAsAsync<ImageResponse>();

        return responseImage;
    }

    private async Task DeleteImage(int loanId, int imageId,
        HttpStatusCode expectedStatusCode = HttpStatusCode.NoContent)
    {
        // perform the action
        var response = await _client.DeleteAsync($"{_apiPath}{loanId}/pickup-protocol/images/{imageId}");

        // Check http status
        Assert.That(response.StatusCode, Is.EqualTo(expectedStatusCode));
    }

    private async Task<ResponseList<ImageResponse>> GetImages(int loanId,
        HttpStatusCode expectedStatusCode = HttpStatusCode.OK)
    {
        // perform the action
        var response = await _client.GetAsync($"{_apiPath}{loanId}/pickup-protocol/images");

        // Check http status
        Assert.That(response.StatusCode, Is.EqualTo(expectedStatusCode));

        // Get the response
        var responseImages = await response.Content.ReadAsAsync<ResponseList<ImageResponse>>();

        return responseImages;
    }

    #endregion
}
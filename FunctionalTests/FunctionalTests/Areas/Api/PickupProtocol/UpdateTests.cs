using System.Net;
using FunctionalTests.Helpers;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using PujcovadloServer;
using PujcovadloServer.Business.Entities;
using PujcovadloServer.Business.Enums;
using PujcovadloServer.Data;
using PujcovadloServer.Requests;
using PujcovadloServer.Responses;
using Xunit;
using Xunit.Abstractions;
using Assert = NUnit.Framework.Assert;

namespace FunctionalTests.FunctionalTests.Areas.Api.PickupProtocol;

[Collection("Sequential")]
public class UpdateTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _application;
    private readonly HttpClient _client;
    private readonly PujcovadloServerContext _db;
    private readonly ITestOutputHelper _output;
    private TestData _data;

    private PujcovadloServer.Business.Entities.Loan _loan;
    private readonly PickupProtocolRequest _protocolUpdateRequest;

    private readonly string _apiPath = "/api/loans/";

    public UpdateTests(CustomWebApplicationFactory<Program> factory, ITestOutputHelper output)
    {
        _application = factory;
        _client = _application.CreateClient();
        _output = output;
        _data = new TestData();

        // get database
        _db = _application.Services.CreateScope().ServiceProvider.GetRequiredService<PujcovadloServerContext>();

        // Reinicialize database base data
        Utilities.ReinitializeDbForTests(_db, _data);

        var loanStartDate = DateTime.Now.AddDays(1);

        // define the loan for this tests
        _loan = new PujcovadloServer.Business.Entities.Loan()
        {
            Item = _data.Item1, Tenant = UserHelper.Tenant, TenantId = UserHelper.TenantId,
            From = loanStartDate, To = loanStartDate.AddDays(2), Status = LoanStatus.Accepted
        };

        _db.Loan.Add(_loan);
        _db.SaveChanges();

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
        var response = await _client.PutAsJsonAsync($"{_apiPath}{_loan.Id}/pickup-protocol",
            _protocolUpdateRequest);

        // Check http status
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Fact]
    public async Task Update_UserHasOnlyUserRole_Forbidden()
    {
        UserHelper.SetAuthorizationHeader(_client, UserHelper.UserToken);

        // Perform the action
        var response = await _client.PutAsJsonAsync($"{_apiPath}{_loan.Id}/pickup-protocol",
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
        var response = await _client.PutAsJsonAsync($"{_apiPath}{_loan.Id}/pickup-protocol",
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
        var response = await _client.PutAsJsonAsync($"{_apiPath}{_loan.Id}/pickup-protocol",
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
        var response = await _client.PutAsJsonAsync($"{_apiPath}{_loan.Id}/pickup-protocol",
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
        var response = await _client.PutAsJsonAsync($"{_apiPath}{_loan.Id}/pickup-protocol",
            _protocolUpdateRequest);

        // Check http status
        Assert.That(response.StatusCode,
            Is.EqualTo(HttpStatusCode.Created)); // Http create because the protocol did not exist

        // Update the protocol because of some mistake
        _protocolUpdateRequest.Description = "All was not ok (updated)";
        response = await _client.PutAsJsonAsync($"{_apiPath}{_loan.Id}/pickup-protocol",
            _protocolUpdateRequest);

        // Check http status
        Assert.That(response.StatusCode,
            Is.EqualTo(HttpStatusCode.NoContent)); // HTTP no content because the protocol already existed

        // Add image
        var image1 = await AddImage(_loan.Id, "FunctionalTests/data/images/img1.jpg");
        var image2 = await AddImage(_loan.Id, "FunctionalTests/data/images/img2.jpg");

        // Get images
        var images = await GetImages(_loan.Id, HttpStatusCode.OK);
        Assert.That(images._data.Count, Is.EqualTo(2));

        // Delete image
        await DeleteImage(_loan.Id, image1.Id, HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Update_UserIsTheOwnerAndStatusIsPickupDenied_Ok()
    {
        UserHelper.SetAuthorizationHeader(_client, UserHelper.OwnerToken);

        // Perform the action
        var response = await _client.PutAsJsonAsync($"{_apiPath}{_loan.Id}/pickup-protocol", _protocolUpdateRequest);

        // Check http status
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created)); // Protocol must have been create before

        // Add image
        var image1 = await AddImage(_loan.Id, "FunctionalTests/data/images/img1.jpg");
        var image2 = await AddImage(_loan.Id, "FunctionalTests/data/images/img2.jpg");

        // Get images
        var images = await GetImages(_loan.Id, HttpStatusCode.OK);
        Assert.That(images._data.Count, Is.EqualTo(2));

        // Delete image
        await DeleteImage(_loan.Id, image1.Id, HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Update_UserIsTheOwnerButStatusDoesNotAllowToUpdateTheProtocol_UnprocessableEntity()
    {
        // Init pickup protocol
        _loan.PickupProtocol = new PujcovadloServer.Business.Entities.PickupProtocol()
        {
            Description = "All was ok",
            AcceptedRefundableDeposit = 200,
            Images = new List<Image>()
            {
                new Image()
                {
                    Name = "test1", Extension = ".jpg", Path = "test1.jpg", OwnerId = UserHelper.OwnerId,
                    Owner = UserHelper.Owner, MimeType = "image/jpeg",
                }
            }
        };

        _db.Update(_loan);
        _db.SaveChanges();

        UserHelper.SetAuthorizationHeader(_client, UserHelper.OwnerToken);

        var loanStatusesWhichDisallowUpdatingTheProtocol = new List<LoanStatus>
        {
            LoanStatus.PreparedForPickup,
            LoanStatus.Active,
            LoanStatus.PreparedForReturn,
            LoanStatus.ReturnDenied,
            LoanStatus.Returned
        };

        foreach (var status in loanStatusesWhichDisallowUpdatingTheProtocol)
        {
            _loan.Status = status;
            _db.Update(_loan);
            _db.SaveChanges();

            // Perform the action
            var response =
                await _client.PutAsJsonAsync($"{_apiPath}{_loan.Id}/pickup-protocol", _protocolUpdateRequest);

            // Check http status
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.UnprocessableEntity));

            // Try to add new image
            await AddImage(_loan.Id, "FunctionalTests/data/images/img1.jpg",
                expectedStatusCode: HttpStatusCode.UnprocessableEntity);

            // Try to delete image
            await DeleteImage(_loan.Id, _loan.PickupProtocol.Images.First().Id,
                expectedStatusCode: HttpStatusCode.UnprocessableEntity);
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
using System.Net;
using FunctionalTests.Helpers;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using PujcovadloServer;
using PujcovadloServer.Business.Entities;
using PujcovadloServer.Business.Enums;
using PujcovadloServer.Data;
using PujcovadloServer.Responses;
using Xunit;
using Xunit.Abstractions;
using Assert = NUnit.Framework.Assert;

namespace FunctionalTests.FunctionalTests.Areas.Api.Item;

public class ManageImagesTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _application;
    private readonly HttpClient _client;
    private readonly PujcovadloServerContext _db;
    private readonly ITestOutputHelper _output;
    private TestData _data;

    private PujcovadloServer.Business.Entities.Item _item;
    /*private readonly PickupProtocolRequest _protocolUpdateRequest;*/

    private readonly string _apiPath = "/api/items/";

    public ManageImagesTests(CustomWebApplicationFactory<Program> factory, ITestOutputHelper output)
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
        _item = new PujcovadloServer.Business.Entities.Item()
        {
            Name = "Item1", Description = "Description1", Status = ItemStatus.Public,
            Parameters = "Parameters1", Owner = UserHelper.Owner, OwnerId = UserHelper.OwnerId, PricePerDay = 100,
            MainImage = null, MainImageId = null,
            Images = new List<Image>()
            {
                new Image()
                {
                    Name = "test1", Extension = ".jpg", Path = "test1.jpg", OwnerId = UserHelper.OwnerId,
                    Owner = UserHelper.Owner, MimeType = "image/jpeg",
                }
            }
        };

        _db.Item.Add(_item);
        _db.SaveChanges();
    }

    #region Authorization

    [Fact]
    public async Task ManageImages_UnauthorizedUser_Unauthorized()
    {
        UserHelper.SetAuthorizationHeader(_client, UserHelper.UnauthorizedUserToken);

        // Perform the action
        await AddImage(_item.Id, "FunctionalTests/data/images/img1.jpg",
            expectedStatusCode: HttpStatusCode.Unauthorized);

        // Perform the action
        await DeleteImage(_item.Id, _item.Images.First().Id, expectedStatusCode: HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ManageImages_UserHasOnlyUserRole_Forbidden()
    {
        UserHelper.SetAuthorizationHeader(_client, UserHelper.UserToken);

        // Perform the action
        await AddImage(_item.Id, "FunctionalTests/data/images/img1.jpg", expectedStatusCode: HttpStatusCode.Forbidden);

        // Perform the action
        await DeleteImage(_item.Id, _item.Images.First().Id, expectedStatusCode: HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ManageImages_UserHasOnlyOwnerRole_Forbidden()
    {
        // Owner2 is not the owner of the item and he cannot update the protocol
        UserHelper.SetAuthorizationHeader(_client, UserHelper.Owner2Token);

        _output.WriteLine("Add image to item " + _item.Id);

        // Perform the action
        await AddImage(_item.Id, "FunctionalTests/data/images/img1.jpg", expectedStatusCode: HttpStatusCode.Forbidden);

        // Perform the action
        await DeleteImage(_item.Id, _item.Images.First().Id, expectedStatusCode: HttpStatusCode.Forbidden);
    }

    #endregion

    #region HelperMethods

    private async Task<ImageResponse> AddImage(int itemId, string path, string mimeType = "image/jpeg",
        HttpStatusCode expectedStatusCode = HttpStatusCode.Created)
    {
        var content = FileUploadHelper.CreateFileUploadForm(path, mimeType);

        // perform the action
        var response = await _client.PostAsync($"{_apiPath}{itemId}/images", content);

        // Check http status
        Assert.That(response.StatusCode, Is.EqualTo(expectedStatusCode));

        // Get the response
        var responseImage = await response.Content.ReadAsAsync<ImageResponse>();

        return responseImage;
    }

    private async Task DeleteImage(int itemId, int imageId,
        HttpStatusCode expectedStatusCode = HttpStatusCode.NoContent)
    {
        // perform the action
        var response = await _client.DeleteAsync($"{_apiPath}{itemId}/images/{imageId}");

        // Check http status
        Assert.That(response.StatusCode, Is.EqualTo(expectedStatusCode));
    }

    private async Task<ResponseList<ImageResponse>> GetImages(int itemId,
        HttpStatusCode expectedStatusCode = HttpStatusCode.OK)
    {
        // perform the action
        var response = await _client.GetAsync($"{_apiPath}{itemId}/images");

        // Check http status
        Assert.That(response.StatusCode, Is.EqualTo(expectedStatusCode));

        // Get the response
        var responseImages = await response.Content.ReadAsAsync<ResponseList<ImageResponse>>();

        return responseImages;
    }

    #endregion
}
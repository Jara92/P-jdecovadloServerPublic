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

namespace FunctionalTests.FunctionalTests.Areas.Api.Item;

public class ItemLifeCycleScenarioTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _application;
    private readonly HttpClient _client;
    private readonly ITestOutputHelper _output;
    private readonly PujcovadloServerContext _db;
    private TestData _data;

    private readonly string _apiPath = "/api/items/";

    private ItemRequest _request;

    public ItemLifeCycleScenarioTests(CustomWebApplicationFactory<Program> factory, ITestOutputHelper output)
    {
        _application = factory;
        _client = _application.CreateClient();
        _output = output;
        _data = new TestData();

        // get database
        _db = _application.Services.CreateScope().ServiceProvider.GetRequiredService<PujcovadloServerContext>();

        // Reinicialize database base data
        Utilities.ReinitializeDbForTests(_db, _data);

        // Define example item request which is valid
        _request = new ItemRequest()
        {
            Name = "item name 1", Description = "Description1", Parameters = "Parameters1", PricePerDay = 100,
            RefundableDeposit = 1000, SellingPrice = 20000, Status = ItemStatus.Public,
            Categories = new List<int>() { 1, 2 }, Tags = new List<string>() { "Tag1", "Tag2" }
        };
    }

    [Fact]
    public async Task SimpleCreateItemScenario()
    {
        UserHelper.SetAuthorizationHeader(_client, UserHelper.OwnerToken);

        // Create item
        var item = await CreateItem();

        // Add images
        var image1 = await AddImage(item.Id, "FunctionalTests/data/images/img1.jpg");
        var image2 = await AddImage(item.Id, "FunctionalTests/data/images/img2.jpg");

        // Get images
        var images = await GetImages(item.Id);
        Assert.That(images._data.Count, Is.EqualTo(2));

        // Remove image
        await DeleteImage(item.Id, image1.Id);
    }

    [Fact]
    public async Task ItemNotApprovedScenario()
    {
        UserHelper.SetAuthorizationHeader(_client, UserHelper.OwnerToken);

        // Create item
        var item = await CreateItem();
        _request.Id = item.Id;

        // Admin does not like the item so he denies it
        var adminItem = _db.Item.Find(item.Id);
        adminItem.Status = ItemStatus.Denied;
        _db.Update(adminItem);
        _db.SaveChanges();

        // Owner now has to update the item
        item = await GetItem(item.Id);

        // Update item
        _request.Name = "New name (hope admin likes it)";
        _request.Description = "New description (hope admin likes it)";

        await UpdateItem(item.Id);
        item = await GetItem(item.Id);

        // Status is still denied because the owner did not change the status
        Assert.That(item.Status, Is.EqualTo(ItemStatus.Denied));

        // Owner updates item again and updates the status to approving
        _request.Status = ItemStatus.Approving;

        await UpdateItem(item.Id);
        item = await GetItem(item.Id);

        // Check that the status is approving
        Assert.That(item.Status, Is.EqualTo(ItemStatus.Approving));

        // Owner add more changes
        _request.Name = "New name (hope admin likes it 2)";

        await UpdateItem(item.Id);
        item = await GetItem(item.Id);

        // Owner adds an image
        var image1 = await AddImage(item.Id, "FunctionalTests/data/images/img1.jpg");

        // Owner wans to set the main image
        _request.MainImageId = image1.Id;
        await UpdateItem(item.Id);
        item = await GetItem(item.Id);

        // Admin likes the item and approves it
        adminItem = _db.Item.Find(item.Id);
        adminItem.Status = ItemStatus.Public;
        _db.Update(adminItem);
        _db.SaveChanges();

        // Owner can do whatever he wants with the item but when admin dislikes it again, the item will be denied

        // Owner deletes the item
        var response = await _client.DeleteAsync($"{_apiPath}{item.Id}");

        // Check http status
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
    }

    #region HelperMethods

    private async Task<ItemDetailResponse> CreateItem()
    {
        // Perform the action
        var response = await _client.PostAsJsonAsync(_apiPath, _request);

        // Check status
        response.EnsureSuccessStatusCode();

        // get data
        var item = await response.Content.ReadAsAsync<ItemDetailResponse>();

        // Check data
        Assert.That(item, Is.Not.Null);
        Assert.That(item.Id, Is.GreaterThan(0));
        Assert.That(item.Name, Is.EqualTo(_request.Name));
        Assert.That(item.Description, Is.EqualTo(_request.Description));
        Assert.That(item.Parameters, Is.EqualTo(_request.Parameters));
        Assert.That(item.PricePerDay, Is.EqualTo(_request.PricePerDay).Within(TestConstants.Tolerance));
        Assert.That(item.RefundableDeposit, Is.EqualTo(_request.RefundableDeposit).Within(TestConstants.Tolerance));
        Assert.That(item.SellingPrice, Is.EqualTo(_request.SellingPrice).Within(TestConstants.Tolerance));
        Assert.That(item.Status, Is.EqualTo(ItemStatus.Public));
        Assert.That(item.Owner.Id, Is.EqualTo(UserHelper.OwnerId));

        return item;
    }

    private async Task<ItemDetailResponse> GetItem(int id)
    {
        var response = await _client.GetAsync($"{_apiPath}{id}");

        // Check status
        response.EnsureSuccessStatusCode();

        // get data
        var item = await response.Content.ReadAsAsync<ItemDetailResponse>();
        return item;
    }

    private async Task UpdateItem(int itemId, HttpStatusCode expectedStatus = HttpStatusCode.NoContent)
    {
        // Perform the action
        var response = await _client.PutAsJsonAsync($"{_apiPath}{itemId}", _request);

        // Check status
        Assert.That(response.StatusCode, Is.EqualTo(expectedStatus));
    }

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
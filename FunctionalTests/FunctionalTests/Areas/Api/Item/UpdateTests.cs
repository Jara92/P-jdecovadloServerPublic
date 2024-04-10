using System.Net;
using System.Net.Http.Json;
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

namespace FunctionalTests.FunctionalTests.Areas.Api.Item;

[Collection("Sequential")]
public class UpdateTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _application;
    private readonly HttpClient _client;
    private readonly PujcovadloServerContext _db;
    private readonly ITestOutputHelper _output;
    private TestData _data;

    private readonly ItemRequest _itemRequest;

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

        // Define example item request which is valid
        _itemRequest = new ItemRequest()
        {
            Id = 1, Name = "Vrtačka Narex zmena", Description = "Je nejlepší a nejvíc modrá vrtačka na trhu zmena.",
            Parameters = "Velikost 5 zmena", PricePerDay = 200,
            RefundableDeposit = 4000, SellingPrice = 10000, PurchasePrice = 15000,
            Categories = new List<int> { 1, 2, 3 }, Tags = { "Vrtacky", "NotNarex" },
            MainImageId = _data.Item1Image1.Id,
            Longitude = 14.42076m, Latitude = 50.08804m
        };
    }

    #region Authorization

    [Fact]
    public async Task Update_UnauthorizedUser_Unauthorized()
    {
        UserHelper.SetAuthorizationHeader(_client, UserHelper.UnauthorizedUserToken);

        // Perform the action
        var response = await _client.PutAsJsonAsync("/api/items/" + _itemRequest.Id, _itemRequest);

        // Check http status
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Fact]
    public async Task Update_UserHasOnlyUserRole_Forbidden()
    {
        UserHelper.SetAuthorizationHeader(_client, UserHelper.UserToken);

        // Perform the action
        var response = await _client.PutAsJsonAsync("/api/items/" + _itemRequest.Id, _itemRequest);

        // Check http status
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
    }

    [Fact]
    public async Task Update_UserHasOnlyTenantRole_Forbidden()
    {
        UserHelper.SetAuthorizationHeader(_client, UserHelper.TenantToken);

        // Perform the action
        var response = await _client.PutAsJsonAsync("/api/items/" + _itemRequest.Id, _itemRequest);

        // Check http status
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
    }

    [Fact]
    public async Task Update_UserHasOwnerRoleButNotTheOwner_Forbidden()
    {
        UserHelper.SetAuthorizationHeader(_client, UserHelper.Owner2Token);

        // Perform the action
        var response = await _client.PutAsJsonAsync("/api/items/" + _itemRequest.Id, _itemRequest);

        // Check http status
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
    }

    #endregion

    [Fact]
    public async Task Update_ValidData_Updated()
    {
        UserHelper.SetAuthorizationHeader(_client, UserHelper.OwnerToken);

        // Perform the action
        var response = await _client.PutAsJsonAsync("/api/items/" + _itemRequest.Id, _itemRequest);

        // Check http status
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

        // Get the item using another request because put request does not return the updated item
        response = await _client.GetAsync("/api/items/" + _itemRequest.Id);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        // Check data
        var result = await response.Content.ReadFromJsonAsync<ItemDetailResponse>();
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<ItemDetailResponse>());

        Assert.That(result.Id, Is.GreaterThan(0));
        Assert.That(result.Name, Is.EqualTo(_itemRequest.Name));
        Assert.That(result.Description, Is.EqualTo(_itemRequest.Description));
        Assert.That(result.Parameters, Is.EqualTo(_itemRequest.Parameters));
        Assert.That(result.PricePerDay, Is.EqualTo(_itemRequest.PricePerDay).Within(TestConstants.Tolerance));
        Assert.That(result.RefundableDeposit,
            Is.EqualTo(_itemRequest.RefundableDeposit).Within(TestConstants.Tolerance));
        Assert.That(result.SellingPrice, Is.EqualTo(_itemRequest.SellingPrice).Within(TestConstants.Tolerance));

        // Check categories
        Assert.That(result.Categories.Count, Is.EqualTo(_itemRequest.Categories.Count));
        for (var i = 0; i < _itemRequest.Categories.Count; i++)
        {
            Assert.That(result.Categories[i].Id, Is.EqualTo(_itemRequest.Categories[i]));
        }

        // Check tags
        Assert.That(result.Tags.Count, Is.EqualTo(_itemRequest.Tags.Count));
        for (var i = 0; i < _itemRequest.Tags.Count; i++)
        {
            Assert.That(result.Tags[i].Name, Is.EqualTo(_itemRequest.Tags[i]));
        }

        // Main image was updated
        Assert.That(result.MainImage, Is.Not.Null);
        Assert.That(result.MainImage.Id, Is.EqualTo(_data.Item1Image1.Id));
    }

    [Fact]
    public async Task Update_SetMainImageToNull_Updated()
    {
        UserHelper.SetAuthorizationHeader(_client, UserHelper.OwnerToken);

        // Update item ID=2
        _itemRequest.Id = 2;

        // Set main image to null
        _itemRequest.MainImageId = null;

        // Perform the action
        var response = await _client.PutAsJsonAsync("/api/items/" + _itemRequest.Id, _itemRequest);

        // Check http status
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

        // Get the item using another request because put request does not return the updated item
        response = await _client.GetAsync("/api/items/" + _itemRequest.Id);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        // Check data
        var result = await response.Content.ReadFromJsonAsync<ItemDetailResponse>();
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<ItemDetailResponse>());

        // Main image was updated
        Assert.That(result.MainImage, Is.Null);
    }

    [Fact]
    public async Task Update_InvalidName_BadRequest()
    {
        UserHelper.SetAuthorizationHeader(_client, UserHelper.OwnerToken);

        // Set invalid data
        _itemRequest.Name = "";

        // Perform the action
        var response = await _client.PutAsJsonAsync("/api/items/" + _itemRequest.Id, _itemRequest);

        // Check http status
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

        // Null name
        _itemRequest.Name = null;

        // Perform the action
        response = await _client.PutAsJsonAsync("/api/items/" + _itemRequest.Id, _itemRequest);

        // Check http status
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Fact]
    public async Task Update_InvalidMainImageId_BadRequest()
    {
        UserHelper.SetAuthorizationHeader(_client, UserHelper.OwnerToken);

        // Set invalid data
        _itemRequest.MainImageId = 90;

        // Perform the action
        var response = await _client.PutAsJsonAsync("/api/items/" + _itemRequest.Id, _itemRequest);

        // Check http status
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Fact]
    public async Task Update_InvalidDescription_BadRequest()
    {
        UserHelper.SetAuthorizationHeader(_client, UserHelper.OwnerToken);

        // Set invalid data
        _itemRequest.Description = "";

        // Perform the action
        var response = await _client.PutAsJsonAsync("/api/items/" + _itemRequest.Id, _itemRequest);

        // Check http status
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

        // Null description
        _itemRequest.Description = null;

        // Perform the action
        response = await _client.PutAsJsonAsync("/api/items/" + _itemRequest.Id, _itemRequest);

        // Check http status
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Fact]
    public async Task Update_NoParameters_Ok()
    {
        UserHelper.SetAuthorizationHeader(_client, UserHelper.OwnerToken);

        // Set invalid data
        _itemRequest.Parameters = "";

        // Perform the action
        var response = await _client.PutAsJsonAsync("/api/items/" + _itemRequest.Id, _itemRequest);

        // Check http status
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

        // Null parameters
        _itemRequest.Parameters = null;

        // Perform the action
        response = await _client.PutAsJsonAsync("/api/items/" + _itemRequest.Id, _itemRequest);

        // Check http status
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
    }

    [Fact]
    public async Task Update_InvalidPricePerDay_BadRequest()
    {
        UserHelper.SetAuthorizationHeader(_client, UserHelper.OwnerToken);

        // Set invalid data
        _itemRequest.PricePerDay = -1;

        // Perform the action
        var response = await _client.PutAsJsonAsync("/api/items/" + _itemRequest.Id, _itemRequest);

        // Check http status
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

        // Null price per day
        _itemRequest.PricePerDay = null;

        // Perform the action
        response = await _client.PutAsJsonAsync("/api/items/" + _itemRequest.Id, _itemRequest);

        // Check http status
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Fact]
    public async Task Update_InvalidRefundableDeposit_BadRequest()
    {
        UserHelper.SetAuthorizationHeader(_client, UserHelper.OwnerToken);

        // Set invalid data
        _itemRequest.RefundableDeposit = -1;

        // Perform the action
        var response = await _client.PutAsJsonAsync("/api/items/" + _itemRequest.Id, _itemRequest);

        // Check http status
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Fact]
    public async Task Update_InvalidSellingPrice_BadRequest()
    {
        UserHelper.SetAuthorizationHeader(_client, UserHelper.OwnerToken);

        // Set invalid data
        _itemRequest.SellingPrice = -1;

        // Perform the action
        var response = await _client.PutAsJsonAsync("/api/items/" + _itemRequest.Id, _itemRequest);

        // Check http status
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Fact]
    public async Task Update_InvalidPurchasePrice_BadRequest()
    {
        UserHelper.SetAuthorizationHeader(_client, UserHelper.OwnerToken);

        // Set invalid data
        _itemRequest.PurchasePrice = -1;

        // Perform the action
        var response = await _client.PutAsJsonAsync("/api/items/" + _itemRequest.Id, _itemRequest);

        // Check http status
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Fact]
    public async Task Update_InvalidCategories_BadRequest()
    {
        UserHelper.SetAuthorizationHeader(_client, UserHelper.OwnerToken);

        // Set invalid data
        _itemRequest.Categories = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

        // Perform the action
        var response = await _client.PutAsJsonAsync("/api/items/" + _itemRequest.Id, _itemRequest);

        // Check http status
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

        // Null categories
        _itemRequest.Categories = null;

        // Perform the action
        response = await _client.PutAsJsonAsync("/api/items/" + _itemRequest.Id, _itemRequest);

        // Check http status
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Fact]
    public async Task Update_InvalidTags_BadRequest()
    {
        UserHelper.SetAuthorizationHeader(_client, UserHelper.OwnerToken);

        // Set invalid data
        _itemRequest.Tags = new List<string> { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11" };

        // Perform the action
        var response = await _client.PutAsJsonAsync("/api/items/" + _itemRequest.Id, _itemRequest);

        // Check http status
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

        // Null tags
        _itemRequest.Tags = null;

        // Perform the action
        response = await _client.PutAsJsonAsync("/api/items/" + _itemRequest.Id, _itemRequest);

        // Check http status
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Fact]
    public async Task Update_ItemDoesNotExist_NotFound()
    {
        UserHelper.SetAuthorizationHeader(_client, UserHelper.OwnerToken);

        // Set invalid data
        _itemRequest.Id = 90;

        // Perform the action
        var response = await _client.PutAsJsonAsync("/api/items/" + _itemRequest.Id, _itemRequest);

        // Check http status
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Fact]
    public async Task Update_ItemIsDeleted_Forbidden()
    {
        UserHelper.SetAuthorizationHeader(_client, UserHelper.OwnerToken);

        // Set invalid data
        _itemRequest.Id = _data.ItemDeleted.Id;

        // Perform the action
        var response = await _client.PutAsJsonAsync("/api/items/" + _itemRequest.Id, _itemRequest);

        // Check http status
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
    }

    [Fact]
    public async Task Update_DifferentQueryIdThanBodyId_BadRequest()
    {
        UserHelper.SetAuthorizationHeader(_client, UserHelper.OwnerToken);

        // Perform the action
        var response = await _client.PutAsJsonAsync("/api/items/" + 90, _itemRequest);

        // Check http status
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }
}
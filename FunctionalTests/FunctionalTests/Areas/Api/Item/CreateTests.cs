using System.Net;
using System.Net.Http.Json;
using FunctionalTests.Helpers;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using NuGet.Packaging;
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
public class CreateTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _application;
    private readonly HttpClient _client;
    private readonly PujcovadloServerContext _db;
    private readonly ITestOutputHelper _output;
    private TestData _data;

    private readonly ItemRequest _itemRequest;

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

        // Define example item request which is valid
        _itemRequest = new ItemRequest
        {
            Name = "Vrtačka Narex", Description = "Je nejlepší a nejvíc modrá vrtačka na trhu.",
            Parameters = "Velikost 5", PricePerDay = 120,
            RefundableDeposit = 2000, SellingPrice = 5000, PurchasePrice = 8000,
            Categories = new List<int> { 1, 2 }, Tags = { "Vrtacky", "Narex" }
        };
    }

    #region Authorization

    [Fact]
    public async Task Create_UnauthorizedUser_Unauthorized()
    {
        UserHelper.SetAuthorizationHeader(_client, UserHelper.UnauthorizedUserToken);

        // Perform the action
        var response = await _client.PostAsJsonAsync("/api/items/", _itemRequest);

        // Check http status
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Fact]
    public async Task Create_UserHasOnlyUserRole_Forbidden()
    {
        UserHelper.SetAuthorizationHeader(_client, UserHelper.UserToken);

        // Perform the action
        var response = await _client.PostAsJsonAsync("/api/items/", _itemRequest);

        // Check http status
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
    }

    [Fact]
    public async Task Create_UserHasOnlyTenantRole_Forbidden()
    {
        UserHelper.SetAuthorizationHeader(_client, UserHelper.TenantToken);

        // Perform the action
        var response = await _client.PostAsJsonAsync("/api/items/", _itemRequest);

        // Check http status
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
    }

    #endregion

    [Fact]
    public async Task Create_ValidData_Created()
    {
        UserHelper.SetAuthorizationHeader(_client, UserHelper.OwnerToken);

        // Perform the action
        var response = await _client.PostAsJsonAsync("/api/items/", _itemRequest);

        // Check http status
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));

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

        // Check images -- no images yet
        Assert.That(result.Images, Is.Empty);
        Assert.That(result.MainImage, Is.Null);
    }

    [Fact]
    public async Task Create_ToManyCategories_BadRequest()
    {
        UserHelper.SetAuthorizationHeader(_client, UserHelper.OwnerToken);

        // Set to many categories
        _itemRequest.Categories = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 };

        // Perform the action
        var response = await _client.PostAsJsonAsync("/api/items/", _itemRequest);

        // Check http status
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Fact]
    public async Task Create_InvalidCategoriesEntered_Created()
    {
        UserHelper.SetAuthorizationHeader(_client, UserHelper.OwnerToken);

        // Add some categories that do not exist
        var invalidCategories = new List<int> { 101, 102 };
        _itemRequest.Categories.AddRange(invalidCategories);

        // Perform the action
        var response = await _client.PostAsJsonAsync("/api/items/", _itemRequest);

        // Check http status - item is created even with invalid categories
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));

        // Check data
        var result = await response.Content.ReadFromJsonAsync<ItemDetailResponse>();
        Assert.That(result, Is.Not.Null);

        // Check that the invalid categories were not added
        Assert.That(result.Categories.Select(c => c.Id),
            Is.EquivalentTo(_itemRequest.Categories.Except(invalidCategories)));
    }

    [Fact]
    public async Task Create_ToManyTags_BadRequest()
    {
        UserHelper.SetAuthorizationHeader(_client, UserHelper.OwnerToken);

        // Set to many categories
        _itemRequest.Tags = new List<string>
        {
            "Tag1", "Tag2", "Tag3", "Tag4", "Tag5", "Tag6", "Tag7", "Tag8", "Tag9", "Tag10", "Tag11", "Tag12", "Tag13",
            "Tag14", "Tag15"
        };

        // Perform the action
        var response = await _client.PostAsJsonAsync("/api/items/", _itemRequest);

        // Check http status
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Fact]
    public async Task Create_NewTagsAdded_Created()
    {
        UserHelper.SetAuthorizationHeader(_client, UserHelper.OwnerToken);

        // Add some tags which already exist in database
        _itemRequest.Tags = new List<string> { _data.ItemTagVrtackaNarex.Name, _data.ItemTagVrtackaBosch.Name };

        // Add some new tags which are not in database yet
        _itemRequest.Tags.AddRange(new[] { "NewTag" });

        // Perform the action
        var response = await _client.PostAsJsonAsync("/api/items/", _itemRequest);

        // Check http status - item is created even with non existing tags
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));

        // Check data
        var result = await response.Content.ReadFromJsonAsync<ItemDetailResponse>();
        Assert.That(result, Is.Not.Null);

        // Check that the result contains all tags
        Assert.That(result.Tags.Select(t => t.Name), Is.EquivalentTo(_itemRequest.Tags.Distinct()));

        // Check that all resulted tags have an id
        Assert.That(result.Tags.Select(t => t.Id), Is.Not.Null);
    }

    [Fact]
    public async Task Create_NoName_BadRequest()
    {
        UserHelper.SetAuthorizationHeader(_client, UserHelper.OwnerToken);

        // Set empty name
        _itemRequest.Name = "";

        // Perform the action
        var response = await _client.PostAsJsonAsync("/api/items/", _itemRequest);

        // Check http status
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

        // set null name
        _itemRequest.Name = null;

        // Perform the action
        response = await _client.PostAsJsonAsync("/api/items/", _itemRequest);

        // Check http status
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Fact]
    public async Task Create_NoDescription_BadRequest()
    {
        UserHelper.SetAuthorizationHeader(_client, UserHelper.OwnerToken);

        // Set empty description
        _itemRequest.Description = "";

        // Perform the action
        var response = await _client.PostAsJsonAsync("/api/items/", _itemRequest);

        // Check http status
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

        // Set null description
        _itemRequest.Description = null;

        // Perform the action
        response = await _client.PostAsJsonAsync("/api/items/", _itemRequest);

        // Check http status
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Fact]
    public async Task Create_InvalidPricePerDay_BadRequest()
    {
        UserHelper.SetAuthorizationHeader(_client, UserHelper.OwnerToken);

        // Price per day is not set
        _itemRequest.PricePerDay = null;

        // Perform the action
        var response = await _client.PostAsJsonAsync("/api/items/", _itemRequest);

        // Check http status
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

        // Price per day is negative
        _itemRequest.PricePerDay = -10;

        // Perform the action
        response = await _client.PostAsJsonAsync("/api/items/", _itemRequest);

        // Check http status
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Fact]
    public async Task Create_NegativeRefundableDeposit_BadRequest()
    {
        UserHelper.SetAuthorizationHeader(_client, UserHelper.OwnerToken);

        // Refundable deposit is negative
        _itemRequest.RefundableDeposit = -10;

        // Perform the action
        var response = await _client.PostAsJsonAsync("/api/items/", _itemRequest);

        // Check http status
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Fact]
    public async Task Create_RefundableDepositNotSet_Created()
    {
        UserHelper.SetAuthorizationHeader(_client, UserHelper.OwnerToken);

        // Refundable deposit is not set
        _itemRequest.RefundableDeposit = null;

        // Perform the action
        var response = await _client.PostAsJsonAsync("/api/items/", _itemRequest);

        // Check http status
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));

        // Check data
        var result = await response.Content.ReadFromJsonAsync<ItemDetailResponse>();
        Assert.That(result, Is.Not.Null);
        Assert.That(result.RefundableDeposit, Is.Null);
    }

    [Fact]
    public async Task Create_NegativeSellingPrice_BadRequest()
    {
        UserHelper.SetAuthorizationHeader(_client, UserHelper.OwnerToken);

        // Selling price is negative
        _itemRequest.SellingPrice = -10;

        // Perform the action
        var response = await _client.PostAsJsonAsync("/api/items/", _itemRequest);

        // Check http status
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Fact]
    public async Task Create_SellingPriceNotSet_Created()
    {
        UserHelper.SetAuthorizationHeader(_client, UserHelper.OwnerToken);

        // Selling price is not set
        _itemRequest.SellingPrice = null;

        // Perform the action
        var response = await _client.PostAsJsonAsync("/api/items/", _itemRequest);

        // Check http status
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));

        // Check data
        var result = await response.Content.ReadFromJsonAsync<ItemDetailResponse>();
        Assert.That(result, Is.Not.Null);
        Assert.That(result.SellingPrice, Is.Null);
    }
}
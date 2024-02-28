using System.Net;
using System.Net.Http.Headers;
using IntegrationTests.Helpers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using PujcovadloServer;
using PujcovadloServer.Data;
using Xunit;
using Xunit.Abstractions;
using Assert = NUnit.Framework.Assert;

namespace IntegrationTests.IntegrationTests.Areas.Api.Controllers;

public class ItemControllerTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _application;
    private readonly HttpClient _client;
    private readonly PujcovadloServerContext _db;
    private readonly ITestOutputHelper _output;

    public ItemControllerTests(CustomWebApplicationFactory<Program> factory, ITestOutputHelper output)
    {
        _application = factory;
        _client = _application.CreateClient();
        _output = output;

        // Arrange
        using (var scope = _application.Services.CreateScope())
        {
            var scopedServices = scope.ServiceProvider;
            _db = scopedServices.GetRequiredService<PujcovadloServerContext>();

            Utilities.ReinitializeDbForTests(_db);
        }
    }

    [Xunit.Theory]
    [InlineData("api/items/")]
    public async Task Index_EndpointsReturnSuccessAndCorrectContentType(string url)
    {
        // Anonymouse user
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, UserHelper.UnauthorizedUserToken);
        var response = await _client.GetAsync(url);
        // Access denied because of missing or invalid token
        NUnit.Framework.Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));

        // user
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, UserHelper.UserToken);
        response = await _client.GetAsync(url);
        // Access granted because of valid token
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        // owner
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, UserHelper.OwnerToken);
        response = await _client.GetAsync(url);
        // Access granted because of valid token
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        //var content = await response.Content.ReadAsStringAsync();
    }

    [Xunit.Theory]
    [InlineData("api/items/1")]
    public async Task GetItem_ItemIsPublic(string url)
    {
        // Anonymouse user
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, UserHelper.UnauthorizedUserToken);
        var response = await _client.GetAsync(url);
        // Access denied because of missing or invalid token
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));

        // user cant see item that is not public
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, UserHelper.UserToken);
        response = await _client.GetAsync(url);
        // Access granted because authenticated user can see public items
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        // owner can see his own item even if it is not public
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, UserHelper.OwnerToken);
        response = await _client.GetAsync(url);
        // Access granted because owner can see his own items
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Xunit.Theory]
    [InlineData("api/items/2")]
    public async Task GetItem_ItemIsNotPublic(string url)
    {
        // Anonymouse user
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, UserHelper.UnauthorizedUserToken);
        var response = await _client.GetAsync(url);
        // Access denied because of missing or invalid token
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));

        // user cant see item that is not public
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, UserHelper.UserToken);
        response = await _client.GetAsync(url);
        // Forbidden because item is not public
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));

        // owner can see his own item even if it is not public
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, UserHelper.OwnerToken);
        response = await _client.GetAsync(url);
        // Access granted because the owner can see his own items
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }
}
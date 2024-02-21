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
using Assert = NUnit.Framework.Assert;

namespace IntegrationTests.IntegrationTests.Item;

public class IndexTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _application;
    private readonly HttpClient _client;

    public IndexTests(CustomWebApplicationFactory<Program> factory)
    {
        _application = factory;
        _client = _application.CreateClient();

        // Arrange
        using (var scope = _application.Services.CreateScope())
        {
            var scopedServices = scope.ServiceProvider;
            var db = scopedServices.GetRequiredService<PujcovadloServerContext>();

            Utilities.ReinitializeDbForTests(db);
        }
    }

    [Xunit.Theory]
    [InlineData("api/items/")]
    [InlineData("api/item-categories/")]
    public async Task Index_EndpointsReturnSuccessAndCorrectContentType(string url)
    {
        // Anonymouse user
        var token = UserHelper.UnauthorizedUserToken;
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, token);
        var response = await _client.GetAsync(url);
        // Access denied because of missing or invalid token
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));

        // user Token
        token = UserHelper.UserToken;
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, token);
        response = await _client.GetAsync(url);
        // Access granted because of valid token
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }
}
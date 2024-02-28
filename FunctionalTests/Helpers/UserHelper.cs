using System.Net.Http.Headers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using PujcovadloServer.Authentication;
using PujcovadloServer.Business.Enums;

namespace FunctionalTests.Helpers;

/// <summary>
/// Helper class to provide user tokens for testing purposes.
/// </summary>
public static class UserHelper
{
    public static string UnauthorizedUserToken => "";

    public static string UserId => "1";

    public static ApplicationUser User = new()
    {
        Id = UserId, UserName = "User", Email = "tester@exmaple.com",
        EmailConfirmed = true, FirstName = "Tester", LastName = "Testovac"
    };

    public static string UserToken => new TestJwtTokenBuilder().WithRole(UserRoles.User).WithEmail("user@example.com")
        .WithUserName("user").WithUserId(UserId).Build();

    public static string OwnerId = "2";

    public static ApplicationUser Owner = new()
    {
        Id = OwnerId, UserName = "Owner", Email = "owner@exmaple.com",
        EmailConfirmed = true, FirstName = "Owner", LastName = "Testovac"
    };

    public static string OwnerToken => new TestJwtTokenBuilder().WithRole(UserRoles.Owner)
        .WithEmail("owner@example.com")
        .WithUserName("owner").WithUserId(OwnerId).Build();

    public static string TenantId = "3";

    public static ApplicationUser Tenant = new()
    {
        Id = TenantId, UserName = "Tenant", Email = "tenant@exmaple.com",
        EmailConfirmed = true, FirstName = "Tenant", LastName = "Testovac"
    };

    public static string TenantToken => new TestJwtTokenBuilder().WithRole(UserRoles.Tenant)
        .WithEmail("tenant@example.com")
        .WithUserName("tenant").WithUserId(TenantId).Build();

    public static void SetAuthorizationHeader(HttpClient client, string token)
    {
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, token);
    }
}
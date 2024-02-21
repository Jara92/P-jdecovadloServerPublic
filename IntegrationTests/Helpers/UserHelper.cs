using PujcovadloServer.Business.Enums;

namespace IntegrationTests.Helpers;

/// <summary>
/// Helper class to provide user tokens for testing purposes.
/// </summary>
public static class UserHelper
{
    public static string UnauthorizedUserToken => "";

    public static string UserId => "1";

    public static string UserToken => new TestJwtTokenBuilder().WithRole(UserRoles.User).WithEmail("user@example.com")
        .WithUserName("user").WithUserId(UserId).Build();
}
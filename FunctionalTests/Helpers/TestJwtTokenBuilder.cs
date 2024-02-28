using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace FunctionalTests.Helpers;

/// <summary>
/// Testing JWT token builder to create a token with specific claims and other properties.
/// </summary>
public class TestJwtTokenBuilder
{
    public List<Claim> Claims { get; } = new();
    public int ExpiresInMinutes { get; set; } = 30;

    public TestJwtTokenBuilder WithUserId(string userId)
    {
        Claims.Add(new Claim(ClaimTypes.PrimarySid, userId));
        return this;
    }

    public TestJwtTokenBuilder WithRole(string roleName)
    {
        Claims.Add(new Claim(ClaimTypes.Role, roleName));
        return this;
    }

    public TestJwtTokenBuilder WithUserName(string username)
    {
        Claims.Add(new Claim(ClaimTypes.Upn, username));
        return this;
    }

    public TestJwtTokenBuilder WithEmail(string email)
    {
        Claims.Add(new Claim(ClaimTypes.Email, email));
        return this;
    }

    public TestJwtTokenBuilder WithExpiration(int expiresInMinutes)
    {
        ExpiresInMinutes = expiresInMinutes;
        return this;
    }

    public string Build()
    {
        var token = new JwtSecurityToken(
            TestJwtTokenProvider.Issuer,
            TestJwtTokenProvider.Issuer,
            Claims,
            expires: DateTime.Now.AddMinutes(ExpiresInMinutes),
            signingCredentials: TestJwtTokenProvider.SigningCredentials
        );
        return TestJwtTokenProvider.JwtSecurityTokenHandler.WriteToken(token);
    }
}
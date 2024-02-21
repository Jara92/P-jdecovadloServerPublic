using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace IntegrationTests.Helpers;

public static class TestJwtTokenProvider
{
    public static string Issuer { get; } = "Sample_Auth_Server";

    /// <summary>
    /// Security key used to sign tokens and validate the signature.
    /// </summary>
    public static SecurityKey SecurityKey { get; } = new SymmetricSecurityKey(
        Encoding.UTF8.GetBytes("Some very long testing key which should be replaced with a real one in production."));

    /// <summary>
    /// the signing credentials used by the token handler to sign tokens
    /// </summary>
    public static SigningCredentials SigningCredentials { get; } =
        new SigningCredentials(SecurityKey, SecurityAlgorithms.HmacSha256);

    /// <summary>
    /// the token handler used to actually issue tokens
    /// </summary>
    public static readonly JwtSecurityTokenHandler JwtSecurityTokenHandler = new();
}
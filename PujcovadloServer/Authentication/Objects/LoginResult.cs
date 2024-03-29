using System.IdentityModel.Tokens.Jwt;

namespace PujcovadloServer.Authentication.Objects;

public class LoginResult
{
    public JwtSecurityToken Token { get; }

    public ApplicationUser CurrentUser { get; }

    public LoginResult(JwtSecurityToken token, ApplicationUser currentUser)
    {
        Token = token;
        CurrentUser = currentUser;
    }
}
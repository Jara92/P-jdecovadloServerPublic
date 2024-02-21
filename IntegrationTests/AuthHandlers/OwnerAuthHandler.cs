using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PujcovadloServer.Business.Enums;

namespace IntegrationTests.AuthHandlers;

public class OwnerAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public OwnerAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
        : base(options, logger, encoder, clock)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, "John Owner"),
            new Claim(ClaimTypes.Sid, "1"),
            new Claim(ClaimTypes.Role, UserRoles.User),
            new Claim(ClaimTypes.Role, UserRoles.Owner)
        };
        var identity = new ClaimsIdentity(claims, "Owner");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "TestScheme");

        var result = AuthenticateResult.Success(ticket);

        return Task.FromResult(result);
    }
}
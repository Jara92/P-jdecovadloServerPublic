using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using PujcovadloServer.Authentication;
using PujcovadloServer.Authentication.Exceptions;
using PujcovadloServer.Business.Enums;

namespace PujcovadloServer.Business.Services.Interfaces;

public interface IAuthenticateService
{
    /// <summary>
    /// Adds new role to the user.
    /// </summary>
    /// <param name="user">User.</param>
    /// <param name="role">Role to be added</param>
    /// <returns></returns>
    public Task AddRole(ApplicationUser user, string role);

    /// <summary>
    /// Registers a new user in the database.
    /// </summary>
    /// <param name="request">Registration request</param>
    /// <exception cref="UserAlreadyExistsException">User with given credentials already exists.</exception>
    /// <exception cref="RegistrationFailedException">Thrown when some other problem occurs.</exception>
    public Task RegisterUser(RegisterRequest request);

    /// <summary>
    /// Performs user login and returns JWT token
    /// </summary>
    /// <param name="request">Login request</param>
    /// <returns>JWT token for the client.</returns>
    /// <exception cref="AuthenticationFailedException">Thrown when entered credentials are not valid.</exception>
    public Task<JwtSecurityToken> Login(LoginRequest request);

    /// <summary>
    /// Returns current user.
    /// </summary>
    /// <returns>Current user entity or null if not authentized.</returns>
    public Task<ApplicationUser?> GetCurrentUser();
}
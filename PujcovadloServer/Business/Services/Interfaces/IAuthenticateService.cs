using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Principal;
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
    /// Is user authenticated?
    /// </summary>
    /// <returns>true is user is authenticated.</returns>
    public bool IsAuthenticated();
    
    /// <summary>
    /// Returns current user.
    /// </summary>
    /// <returns>Current user entity or null if not authentized.</returns>
    /// /// <exception cref="NotAuthenticatedException">Thrown if user is not authenticated.</exception>
    public Task<ApplicationUser> GetCurrentUser();

    /// <summary>
    /// Returns ID of the current user.
    /// </summary>
    /// <returns>Authenticated user id.</returns>
    /// <exception cref="NotAuthenticatedException">Thrown if user is not authenticated.</exception>
    public string GetCurrentUserId();
    
    /// <summary>
    /// Returns ID of the current user or null if not authenticated.
    /// </summary>
    /// <returns>ID of current user or null.</returns>
    public string? TryGetCurrentUserId();
}
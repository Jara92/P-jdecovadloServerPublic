using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using PujcovadloServer.Authentication.Exceptions;
using PujcovadloServer.Authentication.Objects;
using PujcovadloServer.Business.Entities;
using PujcovadloServer.Business.Enums;
using PujcovadloServer.Business.Services.Interfaces;

namespace PujcovadloServer.Authentication;

public class AuthenticateService : IAuthenticateService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IConfiguration _configuration;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuthenticateService(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager,
        IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _configuration = configuration;
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// Create roles if they don't exist.
    /// </summary>
    private async Task CreateRoles()
    {
        // Create roles if they don't exist
        if (!await _roleManager.RoleExistsAsync(UserRoles.Admin))
            await _roleManager.CreateAsync(new IdentityRole(UserRoles.Admin));

        if (!await _roleManager.RoleExistsAsync(UserRoles.User))
            await _roleManager.CreateAsync(new IdentityRole(UserRoles.User));

        if (!await _roleManager.RoleExistsAsync(UserRoles.Tenant))
            await _roleManager.CreateAsync(new IdentityRole(UserRoles.Tenant));

        if (!await _roleManager.RoleExistsAsync(UserRoles.Owner))
            await _roleManager.CreateAsync(new IdentityRole(UserRoles.Owner));
    }

    /// <inheritdoc cref="IAuthenticateService"/>
    public async Task AddRole(ApplicationUser user, string role)
    {
        if (await _roleManager.RoleExistsAsync(role))
        {
            await _userManager.AddToRoleAsync(user, role);
        }
    }

    /// <inheritdoc cref="IAuthenticateService"/>
    public async Task<LoginResult> RegisterUser(RegisterRequest request)
    {
        // Check if user exists
        var usernameExists = await _userManager.FindByNameAsync(request.Username);
        var emailExists = await _userManager.FindByEmailAsync(request.Email);

        if (usernameExists != null)
            throw new UsernameAlreadyExistsException($"Username {request.Username} already exists!");

        if (emailExists != null)
            throw new EmailAlreadyExistsException($"Email {request.Email} already exists!");

        // Check if passwords match
        if (request.Password != request.PasswordConfirmation)
            throw new RegistrationFailedException("Passwords do not match.");

        // Create user using request data
        ApplicationUser user = new ApplicationUser()
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            DateOfBirth = request.DateOfBirth,
            Email = request.Email,
            SecurityStamp = Guid.NewGuid().ToString(),
            UserName = request.Username,
            Profile = new Profile(),
        };

        // Create user in database
        var result = await _userManager.CreateAsync(user, request.Password);

        // Check if user was created successfully
        if (!result.Succeeded)
        {
            throw new RegistrationFailedException(result.Errors.Select(e => e.Description).ToList());
        }

        // Create roles if they don't exist
        await CreateRoles();

        // Add user to role
        await AddRole(user, UserRoles.User);
        //await AddRole(user, UserRoles.Admin);
        await AddRole(user, UserRoles.Tenant);
        await AddRole(user, UserRoles.Owner);

        // Log in the user automatically and return the LoginResponse
        return await Login(new LoginRequest
        {
            Username = user.UserName,
            Password = request.Password
        });
    }

    /// <inheritdoc cref="IAuthenticateService"/>
    public async Task<LoginResult> Login(LoginRequest request)
    {
        // Get user by username
        var user = await _userManager.FindByNameAsync(request.Username);

        // try to find the user by email
        if (user == null)
        {
            user = await _userManager.FindByEmailAsync(request.Username);
        }

        // Check if user exists and password is correct
        if (user != null && await _userManager.CheckPasswordAsync(user, request.Password))
        {
            // Get user roles
            var userRoles = await _userManager.GetRolesAsync(user);

            // Create claims
            var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.PrimarySid, user.Id),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            // Add user roles to claims
            foreach (var userRole in userRoles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, userRole));
            }

            // Create signing key
            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));

            // Get expiration time from configuration
            int expirationTime = _configuration.GetValue<int>("JWT:TokenExpirationInMinutes");

            // Create JWT token
            var token = new JwtSecurityToken(
                issuer: _configuration["JWT:ValidIssuer"],
                audience: _configuration["JWT:ValidAudience"],
                expires: DateTime.Now.AddMinutes(expirationTime),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
            );

            return new LoginResult(token, user);
        }

        throw new AuthenticationFailedException("Invalid username or password.");
    }

    /// <inheritdoc cref="IAuthenticateService"/>
    public bool IsAuthenticated()
    {
        return _httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated ?? false;
    }

    /// <inheritdoc cref="IAuthenticateService"/>
    public async Task<ApplicationUser> GetCurrentUser()
    {
        // Get user id from claims
        var userId = GetCurrentUserId();

        // get the user
        var user = await _userManager.FindByIdAsync(userId);

        // Return user by id
        return user ?? throw new NotAuthenticatedException("User is not authenticated.");
    }

    /// <inheritdoc cref="IAuthenticateService"/>
    public string GetCurrentUserId()
    {
        // Get user id
        var id = TryGetCurrentUserId();

        // Return Id or throw and exception
        return id ?? throw new NotAuthenticatedException("User is not authenticated.");
    }

    /// <inheritdoc cref="IAuthenticateService"/>
    public string? TryGetCurrentUserId()
    {
        // get user id from claims
        var id = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.PrimarySid);

        // Return Id or null
        return id;
    }
}
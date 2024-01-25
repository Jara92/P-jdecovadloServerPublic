using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using PujcovadloServer.Authentication.Exceptions;
using PujcovadloServer.Business.Enums;

namespace PujcovadloServer.Authentication;

public class AuthenticateService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole<int>> _roleManager;
    private readonly IConfiguration _configuration;

    public AuthenticateService(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole<int>> roleManager,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _configuration = configuration;
    }

    private async Task CreateRoles()
    {
        // Create roles if they don't exist
        if (!await _roleManager.RoleExistsAsync(UserRoles.Admin))
            await _roleManager.CreateAsync(new IdentityRole<int>(UserRoles.Admin));

        if (!await _roleManager.RoleExistsAsync(UserRoles.User))
            await _roleManager.CreateAsync(new IdentityRole<int>(UserRoles.User));

        if (!await _roleManager.RoleExistsAsync(UserRoles.Tenant))
            await _roleManager.CreateAsync(new IdentityRole<int>(UserRoles.Tenant));

        if (!await _roleManager.RoleExistsAsync(UserRoles.Owner))
            await _roleManager.CreateAsync(new IdentityRole<int>(UserRoles.Owner));
    }

    public async Task AddRole(ApplicationUser user, string role)
    {
        if (await _roleManager.RoleExistsAsync(role))
        {
            await _userManager.AddToRoleAsync(user, role);
        }
    }

    /// <summary>
    /// Registers a new user in the database.
    /// </summary>
    /// <param name="request">Registration request</param>
    /// <exception cref="UserAlreadyExistsException">User with given credentials already exists.</exception>
    /// <exception cref="RegistrationFailedException">Thrown when some other problem occurs.</exception>
    public async Task RegisterUser(RegisterRequest request)
    {
        // Check if user exists
        var usernameExists = await _userManager.FindByNameAsync(request.Username);
        var emailExists = await _userManager.FindByEmailAsync(request.Email);

        if (usernameExists != null)
            throw new UserAlreadyExistsException($"Username {request.Username} already exists!");

        if (emailExists != null)
            throw new UserAlreadyExistsException($"Email {request.Email} already exists!");

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
            UserName = request.Username
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
    }

    /// <summary>
    /// Performs user login and returns JWT token
    /// </summary>
    /// <param name="request">Login request</param>
    /// <returns>JWT token for the client.</returns>
    /// <exception cref="AuthenticationFailedException">Thrown when entered credentials are not valid.</exception>
    public async Task<JwtSecurityToken> Login(LoginRequest request)
    {
        // Get user by username
        var user = await _userManager.FindByNameAsync(request.Username);

        // Check if user exists and password is correct
        if (user != null && await _userManager.CheckPasswordAsync(user, request.Password))
        {
            // Get user roles
            var userRoles = await _userManager.GetRolesAsync(user);

            // Create claims
            var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            // Add user roles to claims
            foreach (var userRole in userRoles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, userRole));
            }

            // Create JWT token
            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));

            var token = new JwtSecurityToken(
                issuer: _configuration["JWT:ValidIssuer"],
                audience: _configuration["JWT:ValidAudience"],
                expires: DateTime.Now.AddHours(3),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
            );

            return token;
        }

        throw new AuthenticationFailedException("Invalid username or password.");
    }
    
    public async Task<ApplicationUser> GetUser(ClaimsPrincipal principal)
    {
        // Get user id from claims
        var userId = principal.Identity?.Name;
        if (userId == null)
            throw new AuthenticationFailedException("User not authenticated.");

        // Get user by id
        var user = await _userManager.FindByNameAsync(userId);
        if (user == null)
            throw new AuthenticationFailedException("User not authenticated.");
        
        return user;
    }
}
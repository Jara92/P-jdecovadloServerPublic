using AutoMapper;
using Microsoft.AspNetCore.Identity;
using PujcovadloServer.Areas.Admin.Requests;
using PujcovadloServer.Authentication;
using PujcovadloServer.Business.Exceptions;
using PujcovadloServer.Business.Services;

namespace PujcovadloServer.Areas.Admin.Business.Facades;

public class UserFacade
{
    private readonly ApplicationUserService _userService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IMapper _mapper;

    public UserFacade(ApplicationUserService userService, UserManager<ApplicationUser> userManager, IMapper mapper)
    {
        _userService = userService;
        _userManager = userManager;
        _mapper = mapper;
    }

    public async Task<ApplicationUser> Get(string id)
    {
        var user = await _userService.Get(id);
        if (user == null) throw new EntityNotFoundException();

        return user;
    }

    private async Task FillRequest(ApplicationUser user, UserRequest request)
    {
        user.FirstName = request.FirstName;
        user.LastName = request.LastName;

        user.CreatedAt = request.CreatedAt ?? DateTime.Now;

        // Exception if the username is already taken by someone else
        var userWithSameUsername = await _userManager.FindByNameAsync(request.Username);
        if (userWithSameUsername != null && userWithSameUsername.Id != user.Id)
        {
            throw new ArgumentException("Username already exists. Please choose another one.",
                nameof(request.Username));
        }

        user.UserName = request.Username;
        user.DateOfBirth = request.DateOfBirth;
    }

    public async Task<ApplicationUser> Create(UserRequest request)
    {
        var user = new ApplicationUser();
        await FillRequest(user, request);

        // todo: use user manager instead
        await _userService.Create(user);

        // Set user roles after the user has been created
        await SyncUserRoles(user, request.Roles);

        return user;
    }

    public async Task Update(ApplicationUser user, UserRequest request)
    {
        await FillRequest(user, request);

        await _userService.Update(user);

        // Sync user roles after the user has been updated
        await SyncUserRoles(user, request.Roles);
    }

    // TODO: Should be moved to UserManager service
    private async Task SyncUserRoles(ApplicationUser user, IList<string> newRoles)
    {
        var oldRoles = await _userManager.GetRolesAsync(user);

        // Remove roles that are not in the new roles
        var rolesToRemove = oldRoles.Except(newRoles);
        var rolesToAdd = newRoles.Except(oldRoles);

        // Remove roles
        await _userManager.RemoveFromRolesAsync(user, rolesToRemove);

        // Add roles
        await _userManager.AddToRolesAsync(user, rolesToAdd);
    }

    public Task Delete(ApplicationUser user)
    {
        return _userService.Delete(user);
    }

    public Task<IList<string>> GetUserRoles(ApplicationUser user)
    {
        return _userManager.GetRolesAsync(user);
    }
}
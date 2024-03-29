using System.Collections;
using PujcovadloServer.Authentication;
using PujcovadloServer.Business.Enums;
using PujcovadloServer.Business.Filters;
using PujcovadloServer.Business.Interfaces;
using PujcovadloServer.Business.Objects;
using PujcovadloServer.Lib;
using Syncfusion.EJ2.Base;

namespace PujcovadloServer.Business.Services;

public class ApplicationUserService
{
    private readonly IApplicationUserRepository _repository;

    public ApplicationUserService(IApplicationUserRepository repository)
    {
        _repository = repository;
    }

    public virtual async Task<PaginatedList<ApplicationUser>> GetAll(ApplicationUserFilter filter)
    {
        return await _repository.GetAll(filter);
    }

    public Task<List<ApplicationUser>> GetAll(DataManagerRequest dm)
    {
        return _repository.GetAll(dm);
    }

    public Task<IEnumerable> GetAggregations(DataManagerRequest dm)
    {
        return _repository.GetAggregations(dm);
    }

    public Task<int> GetCount(DataManagerRequest dm)
    {
        return _repository.GetCount(dm);
    }

    public virtual Task<ApplicationUser?> Get(string id)
    {
        return _repository.Get(id);
    }

    public Task Update(ApplicationUser item)
    {
        item.UpdatedAt = DateTime.Now;

        return _repository.Update(item);
    }

    public Task Create(ApplicationUser user)
    {
        user.CreatedAt = DateTime.Now;

        return _repository.Create(user);
    }

    public Task Delete(ApplicationUser user)
    {
        user.DeletedAt = DateTime.Now;

        return _repository.Delete(user);
    }

    public Task<IList<ApplicationUserOption>> GetAllAsOptions(ApplicationUserFilter filter)
    {
        return _repository.GetAllAsOptions(filter);
    }

    public dynamic GetRoleOptions()
    {
        return UserRoles.AllRoles.Select(r => new RoleOption
        {
            Id = r,
            Name = r
        });
    }
}
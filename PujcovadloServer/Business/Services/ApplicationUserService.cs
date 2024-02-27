using PujcovadloServer.Authentication;
using PujcovadloServer.Business.Filters;
using PujcovadloServer.Business.Interfaces;
using PujcovadloServer.Business.Objects;
using PujcovadloServer.Lib;

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

    public Task<ApplicationUser?> Get(string id)
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
}
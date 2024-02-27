using PujcovadloServer.Authentication;
using PujcovadloServer.Business.Filters;
using PujcovadloServer.Business.Objects;
using PujcovadloServer.Lib;

namespace PujcovadloServer.Business.Interfaces;

public interface IApplicationUserRepository
{
    public Task<PaginatedList<ApplicationUser>> GetAll(ApplicationUserFilter filter);

    public Task<ApplicationUser?> Get(string id);

    public Task Create(ApplicationUser user);

    public Task Update(ApplicationUser user);

    public Task Delete(ApplicationUser user);

    Task<IList<ApplicationUserOption>> GetAllAsOptions(ApplicationUserFilter filter);
}
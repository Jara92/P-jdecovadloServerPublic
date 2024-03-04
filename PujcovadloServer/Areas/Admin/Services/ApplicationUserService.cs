using Microsoft.EntityFrameworkCore;
using PujcovadloServer.Areas.Admin.Business.Objects;
using PujcovadloServer.Data;

namespace PujcovadloServer.Areas.Admin.Services;

public class ApplicationUserService
{
    private readonly PujcovadloServerContext _dbContext;

    public ApplicationUserService(PujcovadloServerContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<List<UserOption>> GetUserOptions()
    {
        return _dbContext.Users.Select(u => new UserOption()
        {
            Id = u.Id,
            Name = u.FirstName + " " + u.LastName
        }).ToListAsync();
    }
}
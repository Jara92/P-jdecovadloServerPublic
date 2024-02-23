using Microsoft.EntityFrameworkCore;
using PujcovadloServer.Business.Entities;
using PujcovadloServer.Business.Filters;
using PujcovadloServer.Business.Interfaces;

namespace PujcovadloServer.Data.Repositories;

public class ProfileRepository : ACrudRepository<Profile, ProfileFilter>, IProfileRepository
{
    public ProfileRepository(PujcovadloServerContext context) : base(context)
    {
    }

    public async Task<IList<Image>> GetByIds(IEnumerable<int> ids)
    {
        return await _context.Image.Where(c => ids.Contains(c.Id)).ToListAsync();
    }

    public async Task<Image?> GetByPath(string name)
    {
        return await _context.Image.FirstOrDefaultAsync(c => c.Path == name);
    }
}
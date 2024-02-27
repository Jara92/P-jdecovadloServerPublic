using Microsoft.EntityFrameworkCore;
using PujcovadloServer.Business.Entities;
using PujcovadloServer.Business.Filters;
using PujcovadloServer.Business.Interfaces;
using PujcovadloServer.Business.Objects;

namespace PujcovadloServer.Data.Repositories;

public class ItemTagRepository : ACrudRepository<ItemTag, ItemTagFilter>, IItemTagRepository
{
    public ItemTagRepository(PujcovadloServerContext context) : base(context)
    {
    }

    public async Task<ItemTag?> GetByName(string name)
    {
        return await _dbSet.Where(c => c.Name == name).FirstOrDefaultAsync();
    }

    public async Task<IList<ItemTagOption>> GetAllAsOptions()
    {
        return await _dbSet.Select(c => new ItemTagOption
        {
            Id = c.Id,
            Name = c.Name
        }).ToListAsync();
    }

    public async Task<IEnumerable<ItemTag>> GetByIds(ICollection<int> requestTags)
    {
        return await _dbSet.Where(c => requestTags.Contains(c.Id)).ToListAsync();
    }
}
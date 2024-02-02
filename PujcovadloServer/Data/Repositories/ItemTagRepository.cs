using Microsoft.EntityFrameworkCore;
using PujcovadloServer.Business.Entities;
using PujcovadloServer.Business.Filters;
using PujcovadloServer.Business.Interfaces;

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
}
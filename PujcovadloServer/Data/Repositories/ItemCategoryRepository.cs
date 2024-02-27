using Microsoft.EntityFrameworkCore;
using PujcovadloServer.Business.Entities;
using PujcovadloServer.Business.Filters;
using PujcovadloServer.Business.Interfaces;
using PujcovadloServer.Business.Objects;

namespace PujcovadloServer.Data.Repositories;

public class ItemCategoryRepository : ACrudRepository<ItemCategory, ItemCategoryFilter>, IItemCategoryRepository
{
    public ItemCategoryRepository(PujcovadloServerContext context) : base(context)
    {
    }

    public async Task<IList<ItemCategory>> GetByIds(IEnumerable<int> ids)
    {
        return await _context.ItemCategory.Where(c => ids.Contains(c.Id)).ToListAsync();
    }

    public async Task<IList<ItemCategoryOption>> GetAllOptions()
    {
        return await _dbSet.Select(c => new ItemCategoryOption
        {
            Id = c.Id,
            Name = c.Name
        }).ToListAsync();
    }
}
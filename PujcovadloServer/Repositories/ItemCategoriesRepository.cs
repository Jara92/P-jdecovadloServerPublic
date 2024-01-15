using Microsoft.EntityFrameworkCore;
using PujcovadloServer.data;
using PujcovadloServer.Models;

namespace PujcovadloServer.Repositories;

public class ItemCategoriesRepository
{
    private readonly PujcovadloServerContext _context;
    private readonly DbSet<ItemCategory> _dbSet;

    public ItemCategoriesRepository(PujcovadloServerContext context)
    {
        _context = context;
        _dbSet = context.ItemCategory;
    }

    public async Task<List<ItemCategory>> GetAll()
    {
        return await _dbSet.ToListAsync();
    }

    public async Task<ItemCategory?> Get(int id)
    {
        return await _dbSet.FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task Create(ItemCategory category)
    {
        await _dbSet.AddAsync(category);
        await _context.SaveChangesAsync();
    }

    public async Task Update(ItemCategory category)
    {
        _context.Update(category);
        await _context.SaveChangesAsync();
    }

    public async Task Delete(ItemCategory category)
    {
        _context.Remove(category);
        await _context.SaveChangesAsync();
    }
}
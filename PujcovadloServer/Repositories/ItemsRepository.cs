using Microsoft.EntityFrameworkCore;
using PujcovadloServer.data;
using PujcovadloServer.Models;

namespace PujcovadloServer.Repositories;

public class ItemsRepository
{
    private readonly PujcovadloServerContext _context;
    private readonly DbSet<Item> _dbSet;

    public ItemsRepository(PujcovadloServerContext context)
    {
        _context = context;
        _dbSet = context.Item;
    }

    public async Task<List<Item>> GetAll()
    {
        return await _dbSet.ToListAsync();
    }

    public async Task<Item?> Get(int id)
    {
        return await _dbSet.FirstOrDefaultAsync(m => m.Id == id);
    }

    public async void Create(Item item)
    {
        item.CreatedAt = DateTime.Now;

        await _dbSet.AddAsync(item);
        await _context.SaveChangesAsync();
    }

    public async void Update(Item item)
    {
        item.UpdatedAt = DateTime.Now;

        _context.Update(item);
        await _context.SaveChangesAsync();
    }

    public async void Delete(Item item)
    {
        _context.Remove(item);
        await _context.SaveChangesAsync();
    }
}
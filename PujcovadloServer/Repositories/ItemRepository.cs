using Microsoft.EntityFrameworkCore;
using PujcovadloServer.data;
using PujcovadloServer.Exceptions;
using PujcovadloServer.Models;
using PujcovadloServer.Repositories.Interfaces;

namespace PujcovadloServer.Repositories;

public class ItemRepository : ACrudRepository<Item>, IItemRepository
{
    private readonly PujcovadloServerContext _context;
    private readonly DbSet<Item> _dbSet;

    public ItemRepository(PujcovadloServerContext context) : base(context)
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
        return await _dbSet.
            // Include(i => i.ItemCategories).
            FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task Create(Item item)
    {
        item.CreatedAt = DateTime.Now;

        await _dbSet.AddAsync(item);
        await _context.SaveChangesAsync();
    }

    public async Task Update(Item item)
    {
        var oldItem = await Get(item.Id);

        if (oldItem == null)
        {
            throw new EntityNotFoundException();
        }

        // Update only changed values.
        _context.Entry(oldItem).CurrentValues.SetValues(item);

        // Update timestamps.
        item.UpdatedAt = DateTime.Now;

        _context.Update(item);
        await _context.SaveChangesAsync();
    }

    public async Task Delete(Item item)
    {
        _context.Remove(item);
        await _context.SaveChangesAsync();
    }
}
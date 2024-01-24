using Microsoft.EntityFrameworkCore;
using PujcovadloServer.Business.Entities;
using PujcovadloServer.Business.Exceptions;
using PujcovadloServer.Business.Filters;
using PujcovadloServer.Business.Interfaces;
using PujcovadloServer.Lib;

namespace PujcovadloServer.Data.Repositories;

public class ItemRepository : ACrudRepository<Item, ItemFilter>, IItemRepository
{
    private readonly PujcovadloServerContext _context;
    private readonly DbSet<Item> _dbSet;

    public ItemRepository(PujcovadloServerContext context) : base(context)
    {
        _context = context;
        _dbSet = context.Item;
    }

    // TODO: get all is not actually overriding the base method because of the different parameter type.
    
    public async Task<PaginatedList<Item>> GetAll(ItemFilter filter)
    {
        var query = _dbSet.AsQueryable();
        
        // Filter by status
        if(filter.Status != null)
            query = query.Where(i => i.Status == filter.Status);
        
        // Filter by category
        if(filter.Category != null)
            query = query.Where(i => i.Categories.Contains(filter.Category));
        
        // Search by name or description
        if(filter.Search != null)
            query = query.Where(i => i.Name.Contains(filter.Search) || i.Description.Contains(filter.Search));

        switch (filter.Sortby)
        {
            case "Id":
                query = filter.SortOrder == true ? query.OrderBy(i => i.Id) : query.OrderByDescending(i => i.Id);
                break;
        }

        // Return paginated result
        return await base.GetAll(query, filter);
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
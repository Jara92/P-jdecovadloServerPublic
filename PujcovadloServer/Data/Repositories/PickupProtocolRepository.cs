using Microsoft.EntityFrameworkCore;
using PujcovadloServer.Business.Entities;
using PujcovadloServer.Business.Filters;
using PujcovadloServer.Business.Interfaces;

namespace PujcovadloServer.Data.Repositories;

public class PickupProtocolRepository : ACrudRepository<PickupProtocol, BaseFilter>, IPickupProtocolRepository
{
    private readonly PujcovadloServerContext _context;
    private readonly DbSet<PickupProtocol> _dbSet;

    public PickupProtocolRepository(PujcovadloServerContext context) : base(context)
    {
        _context = context;
        _dbSet = context.PickupProtocol;
    }
}
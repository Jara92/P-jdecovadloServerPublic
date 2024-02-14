using Microsoft.EntityFrameworkCore;
using PujcovadloServer.Business.Entities;
using PujcovadloServer.Business.Filters;
using PujcovadloServer.Business.Interfaces;

namespace PujcovadloServer.Data.Repositories;

public class ReturnProtocolRepository : ACrudRepository<ReturnProtocol, BaseFilter>, IReturnProtocolRepository
{
    private readonly PujcovadloServerContext _context;
    private readonly DbSet<ReturnProtocol> _dbSet;

    public ReturnProtocolRepository(PujcovadloServerContext context) : base(context)
    {
        _context = context;
        _dbSet = context.ReturnProtocol;
    }
}
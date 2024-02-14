using PujcovadloServer.Business.Entities;
using PujcovadloServer.Business.Filters;

namespace PujcovadloServer.Business.Interfaces;

public interface IReturnProtocolRepository : ICrudRepository<ReturnProtocol, BaseFilter>
{
}
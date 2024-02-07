using PujcovadloServer.Business.Entities;
using PujcovadloServer.Business.Filters;

namespace PujcovadloServer.Business.Interfaces;

public interface IPickupProtocolRepository : ICrudRepository<PickupProtocol, BaseFilter>
{
}
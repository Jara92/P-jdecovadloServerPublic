using PujcovadloServer.Business.Entities;
using PujcovadloServer.Business.Filters;

namespace PujcovadloServer.Business.Interfaces;

public interface IProfileRepository : ICrudRepository<Profile, ProfileFilter>
{
}
using PujcovadloServer.Business.Entities;
using PujcovadloServer.Business.Filters;
using PujcovadloServer.Business.Interfaces;

namespace PujcovadloServer.Business.Services;

public class ProfileService(IProfileRepository repository)
    : ACrudService<Profile, IProfileRepository, ProfileFilter>(repository)
{
}
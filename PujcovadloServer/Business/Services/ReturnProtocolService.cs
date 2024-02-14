using PujcovadloServer.Business.Entities;
using PujcovadloServer.Business.Filters;
using PujcovadloServer.Business.Interfaces;

namespace PujcovadloServer.Business.Services;

public class ReturnProtocolService : ACrudService<ReturnProtocol, IReturnProtocolRepository, BaseFilter>
{
    public ReturnProtocolService(IReturnProtocolRepository repository) : base(repository)
    {
    }

    public override Task Update(ReturnProtocol entity)
    {
        entity.UpdatedAt = DateTime.Now;

        return base.Update(entity);
    }
}
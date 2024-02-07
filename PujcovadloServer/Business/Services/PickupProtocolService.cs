using PujcovadloServer.Business.Entities;
using PujcovadloServer.Business.Filters;
using PujcovadloServer.Business.Interfaces;

namespace PujcovadloServer.Business.Services;

public class PickupProtocolService : ACrudService<PickupProtocol, IPickupProtocolRepository, BaseFilter>
{
    
    public PickupProtocolService(IPickupProtocolRepository repository) : base(repository)
    {
    }

    public override Task Update(PickupProtocol entity)
    {
        entity.UpdatedAt = DateTime.Now;
        
        return base.Update(entity);
    }
}
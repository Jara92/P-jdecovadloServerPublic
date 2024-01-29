using PujcovadloServer.Business.Entities;

namespace PujcovadloServer.Business.States;

public interface IState<in T, in TG> where T : BaseEntity where TG : Enum
{
    public void Handle(T entity, TG newStatus);
}
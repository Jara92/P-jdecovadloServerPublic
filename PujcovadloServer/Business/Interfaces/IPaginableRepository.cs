using System.Collections;
using Syncfusion.EJ2.Base;

namespace PujcovadloServer.Business.Interfaces;

public interface IPaginableRepository<T>
{
    public Task<List<T>> GetAll(DataManagerRequest dm);

    public Task<IEnumerable> GetAggregations(DataManagerRequest dm);

    public Task<int> GetCount(DataManagerRequest dm);
}
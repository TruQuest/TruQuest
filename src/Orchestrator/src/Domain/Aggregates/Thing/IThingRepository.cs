using Domain.Base;

namespace Domain.Aggregates;

public interface IThingRepository : IRepository<Thing>
{
    void Create(Thing thing);
}
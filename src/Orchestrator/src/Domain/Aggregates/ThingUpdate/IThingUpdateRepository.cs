using Domain.Base;

namespace Domain.Aggregates;

public interface IThingUpdateRepository : IRepository<ThingUpdate>
{
    Task AddOrUpdate(params ThingUpdate[] updateEvents);
}
using Domain.Base;

namespace Domain.Aggregates;

public interface IThingUpdateRepository : IRepository<ThingUpdate>
{
    void Add(ThingUpdate updateEvent);
}
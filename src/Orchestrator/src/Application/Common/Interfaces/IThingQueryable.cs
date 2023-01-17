using Application.Thing.Queries.GetThing;

namespace Application.Common.Interfaces;

public interface IThingQueryable
{
    Task<ThingQm?> GetById(Guid id);
}
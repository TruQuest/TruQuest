using Domain.Base;

namespace Domain.Aggregates;

public interface IThingRepository : IRepository<Thing>
{
    void Create(Thing thing);
    Task<Thing> FindById(Guid id);
    Task<ThingState> GetStateFor(Guid thingId);
    Task UpdateStateFor(Guid thingId, ThingState state);
    Task<bool> CheckIsDesignatedVerifierFor(Guid thingId, string userId);
    Task<IReadOnlyList<ThingVerifier>> GetAllVerifiersFor(Guid thingId);
    Task<Guid> DeepCopyFromWith(Guid sourceThingId, ThingState state);
}

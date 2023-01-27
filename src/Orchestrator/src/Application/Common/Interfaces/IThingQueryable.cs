using Application.Common.Models.QM;
using Application.Thing.Queries.GetThing;

namespace Application.Common.Interfaces;

public interface IThingQueryable
{
    Task<ThingQm?> GetById(Guid id);

    Task<IEnumerable<VerifierLotteryParticipantEntryQm>> GetVerifierLotteryParticipants(
        Guid thingId
    );

    Task<IEnumerable<VerifierQm>> GetVerifiers(Guid thingId);
}
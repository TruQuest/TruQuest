using Application.Common.Models.QM;
using Application.Subject.Common.Models.QM;
using Application.Thing.Queries.GetThing;

namespace Application.Common.Interfaces;

public interface IThingQueryable
{
    Task<IEnumerable<ThingPreviewQm>> GetForSubject(Guid subjectId, string? userId);

    Task<ThingQm?> GetById(Guid id, string? userId);

    Task<IEnumerable<VerifierLotteryParticipantEntryQm>> GetVerifierLotteryParticipants(
        Guid thingId
    );

    Task<IEnumerable<string>> GetVerifiers(Guid thingId);
}

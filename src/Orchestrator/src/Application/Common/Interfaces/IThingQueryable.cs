using Domain.Aggregates;

using Application.Subject.Common.Models.QM;
using Application.Thing.Queries.GetThing;
using Application.Admin.Queries.GetContractsStates.QM;

namespace Application.Common.Interfaces;

public interface IThingQueryable
{
    Task<IEnumerable<ThingPreviewQm>> GetForSubject(Guid subjectId, string? userId);
    Task<ThingQm?> GetById(Guid id, string? userId);
    Task<ThingState> GetStateFor(Guid thingId);
    Task<IEnumerable<ThingTitleAndSubjectInfoQm>> GetTitleAndSubjectInfoFor(IEnumerable<Guid> thingIds);
}

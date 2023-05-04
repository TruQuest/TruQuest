using Application.Subject.Common.Models.QM;

namespace Application.Subject.Queries.GetThingsList;

public class GetThingsListResultVm
{
    public required Guid SubjectId { get; init; }
    public required IEnumerable<ThingPreviewQm> Things { get; init; }
}
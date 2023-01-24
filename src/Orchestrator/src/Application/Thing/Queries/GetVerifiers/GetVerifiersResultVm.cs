using Application.Common.Models.QM;

namespace Application.Thing.Queries.GetVerifiers;

public class GetVerifiersResultVm
{
    public required Guid ThingId { get; init; }
    public required IEnumerable<VerifierQm> Verifiers { get; init; }
}
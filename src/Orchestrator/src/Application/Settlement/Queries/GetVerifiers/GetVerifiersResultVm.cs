using Application.Common.Models.QM;

namespace Application.Settlement.Queries.GetVerifiers;

public class GetVerifiersResultVm
{
    public required Guid ProposalId { get; init; }
    public required IEnumerable<VerifierQm> Verifiers { get; init; }
}
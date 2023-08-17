using Application.Common.Models.QM;

namespace Application.Settlement.Queries.GetVotes;

public class GetVotesResultVm
{
    public required Guid ProposalId { get; init; }
    public required string? VoteAggIpfsCid { get; init; }
    public required IEnumerable<VoteQm> Votes { get; init; }
}

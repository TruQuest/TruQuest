using Application.Common.Models.QM;

namespace Application.Thing.Queries.GetVotes;

public class GetVotesResultVm
{
    public required Guid ThingId { get; init; }
    public required string? VoteAggIpfsCid { get; init; }
    public required IEnumerable<Vote2Qm> Votes { get; init; }
}

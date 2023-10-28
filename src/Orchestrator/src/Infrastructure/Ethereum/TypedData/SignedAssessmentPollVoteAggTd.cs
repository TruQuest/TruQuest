namespace Infrastructure.Ethereum.TypedData;

public class SignedAssessmentPollVoteAggTd
{
    public required Guid ThingId { get; init; }
    public required Guid SettlementProposalId { get; init; }
    public required ulong L1EndBlock { get; init; }
    public required List<OffChainAssessmentPollVoteTd> OffChainVotes { get; init; }
    public required List<OnChainAssessmentPollVoteTd> OnChainVotes { get; init; }
}

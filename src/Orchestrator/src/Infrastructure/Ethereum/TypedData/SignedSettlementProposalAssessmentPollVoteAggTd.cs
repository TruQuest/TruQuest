namespace Infrastructure.Ethereum.TypedData;

public class SignedSettlementProposalAssessmentPollVoteAggTd
{
    public required Guid ThingId { get; init; }
    public required Guid SettlementProposalId { get; init; }
    public required ulong L1EndBlock { get; init; }
    public required List<OffChainSettlementProposalAssessmentPollVoteTd> OffChainVotes { get; init; }
    public required List<OnChainSettlementProposalAssessmentPollVoteTd> OnChainVotes { get; init; }
}

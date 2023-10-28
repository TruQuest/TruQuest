namespace Infrastructure.Ethereum.TypedData;

public class SignedAcceptancePollVoteAggTd
{
    public required Guid ThingId { get; init; }
    public required ulong L1EndBlock { get; init; }
    public required List<OffChainAcceptancePollVoteTd> OffChainVotes { get; init; }
    public required List<OnChainAcceptancePollVoteTd> OnChainVotes { get; init; }
}

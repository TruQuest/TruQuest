namespace Infrastructure.Ethereum.TypedData;

public class SignedThingValidationPollVoteAggTd
{
    public required Guid ThingId { get; init; }
    public required ulong L1EndBlock { get; init; }
    public required List<OffChainThingValidationPollVoteTd> OffChainVotes { get; init; }
    public required List<OnChainThingValidationPollVoteTd> OnChainVotes { get; init; }
}

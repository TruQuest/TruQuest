namespace Application.Thing.Queries.GetVerifierLotteryParticipants;

public class VerifierLotteryParticipantEntryQm
{
    public long? JoinedBlockNumber { get; }
    public string UserId { get; }
    public string DataHash { get; }
    public decimal? Nonce { get; }
}
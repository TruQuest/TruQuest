namespace Application.Common.Models.QM;

public class VerifierLotteryParticipantEntryQm
{
    public long? JoinedBlockNumber { get; }
    public string UserId { get; }
    public string DataHash { get; }
    public decimal? Nonce { get; }
}
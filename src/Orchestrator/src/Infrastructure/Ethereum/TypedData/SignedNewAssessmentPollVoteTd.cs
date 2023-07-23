using Application.Settlement.Commands.CastAssessmentPollVote;

namespace Infrastructure.Ethereum.TypedData;

public class SignedNewAssessmentPollVoteTd
{
    public required NewAssessmentPollVoteIm Vote { get; init; }
    public required string WalletAddress { get; init; }
    public required string OwnerAddress { get; init; }
    public required string OwnerSignature { get; init; }
}

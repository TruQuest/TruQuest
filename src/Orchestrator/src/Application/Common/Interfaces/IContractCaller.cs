using System.Numerics;

namespace Application.Common.Interfaces;

public interface IContractCaller
{
    Task<byte[]> ComputeHashForThingSubmissionVerifierLottery(byte[] data);
    Task<long> InitVerifierLottery(string thingId, byte[] dataHash);
    Task<BigInteger> ComputeNonce(string thingId, byte[] data);
    Task CloseVerifierLotteryWithSuccess(string thingId, byte[] data, List<ulong> winnerIndices);
    Task FinalizeAcceptancePollForThingAsAccepted(
        string thingId, string voteAggIpfsCid,
        List<string> verifiersToReward, List<string> verifiersToSlash
    );
    Task<long> InitThingAssessmentVerifierLottery(string thingId, byte[] dataHash);
    Task<BigInteger> ComputeNonceForThingAssessmentVerifierLottery(string thingId, byte[] data);
    Task<byte[]> ComputeHashForThingAssessmentVerifierLottery(byte[] data);
    Task CloseThingAssessmentVerifierLotteryWithSuccess(
        string thingId, byte[] data, List<ulong> winnerClaimantIndices, List<ulong> winnerIndices
    );
}
using System.Numerics;

namespace Application.Common.Interfaces;

public interface IContractCaller
{
    Task<byte[]> ComputeHashForThingSubmissionVerifierLottery(byte[] data);
    Task<long> InitVerifierLottery(byte[] thingId, byte[] dataHash);
    Task<BigInteger> ComputeNonce(byte[] thingId, byte[] data);
    Task CloseVerifierLotteryWithSuccess(byte[] thingId, byte[] data, List<ulong> winnerIndices);
    Task FinalizeAcceptancePollForThingAsAccepted(
        byte[] thingId, string voteAggIpfsCid,
        List<string> verifiersToReward, List<string> verifiersToSlash
    );
    Task<long> InitThingAssessmentVerifierLottery(byte[] thingId, byte[] dataHash);
    Task<BigInteger> ComputeNonceForThingAssessmentVerifierLottery(byte[] thingId, byte[] data);
    Task<byte[]> ComputeHashForThingAssessmentVerifierLottery(byte[] data);
    Task CloseThingAssessmentVerifierLotteryWithSuccess(
        byte[] thingId, byte[] data, List<ulong> winnerClaimantIndices, List<ulong> winnerIndices
    );
}
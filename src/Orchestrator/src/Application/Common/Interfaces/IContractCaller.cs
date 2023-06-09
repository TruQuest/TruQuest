using System.Numerics;

namespace Application.Common.Interfaces;

public interface IContractCaller
{
    Task<byte[]> ComputeHashForThingSubmissionVerifierLottery(byte[] data);
    Task<long> InitThingSubmissionVerifierLottery(byte[] thingId, byte[] dataHash, byte[] userXorDataHash);
    Task<long> GetThingSubmissionVerifierLotteryInitBlock(byte[] thingId);
    Task<bool> CheckThingSubmissionVerifierLotteryExpired(byte[] thingId);
    Task<BigInteger> GetThingSubmissionVerifierLotteryMaxNonce();
    Task CloseThingSubmissionVerifierLotteryWithSuccess(
        byte[] thingId, byte[] data, byte[] userXorData, byte[] hashOfL1EndBlock, List<ulong> winnerIndices
    );
    Task CloseThingSubmissionVerifierLotteryInFailure(byte[] thingId, int joinedNumVerifiers);

    Task<long> GetThingAcceptancePollInitBlock(byte[] thingId);
    Task FinalizeAcceptancePollForThingAsUnsettledDueToInsufficientVotingVolume(
        byte[] thingId, string voteAggIpfsCid, List<ulong> verifiersToSlashIndices
    );
    Task FinalizeAcceptancePollForThingAsUnsettledDueToMajorityThresholdNotReached(
        byte[] thingId, string voteAggIpfsCid, List<ulong> verifiersToSlashIndices
    );
    Task FinalizeAcceptancePollForThingAsAccepted(
        byte[] thingId, string voteAggIpfsCid, List<ulong> verifiersToSlashIndices
    );
    Task FinalizeAcceptancePollForThingAsSoftDeclined(
        byte[] thingId, string voteAggIpfsCid, List<ulong> verifiersToSlashIndices
    );
    Task FinalizeAcceptancePollForThingAsHardDeclined(
        byte[] thingId, string voteAggIpfsCid, List<ulong> verifiersToSlashIndices
    );
    Task<IEnumerable<string>> GetVerifiersForThing(byte[] thingId);

    Task<byte[]> ComputeHashForThingAssessmentVerifierLottery(byte[] data);
    Task<long> InitThingAssessmentVerifierLottery(byte[] thingId, byte[] proposalId, byte[] dataHash, byte[] userXorDataHash);
    Task<long> GetThingAssessmentVerifierLotteryInitBlock(byte[] thingId, byte[] proposalId);
    Task<bool> CheckThingAssessmentVerifierLotteryExpired(byte[] thingId, byte[] proposalId);
    Task<BigInteger> GetThingAssessmentVerifierLotteryMaxNonce();
    Task CloseThingAssessmentVerifierLotteryWithSuccess(
        byte[] thingId, byte[] proposalId, byte[] data, byte[] userXorData,
        byte[] hashOfL1EndBlock, List<ulong> winnerClaimantIndices, List<ulong> winnerIndices
    );
    Task CloseThingAssessmentVerifierLotteryInFailure(byte[] thingId, byte[] proposalId, int joinedNumVerifiers);

    Task<long> GetThingAssessmentPollInitBlock(byte[] thingId, byte[] proposalId);
    Task FinalizeAssessmentPollForProposalAsUnsettledDueToInsufficientVotingVolume(
        byte[] thingId, byte[] proposalId, string voteAggIpfsCid, List<ulong> verifiersToSlashIndices
    );
    Task FinalizeAssessmentPollForProposalAsUnsettledDueToMajorityThresholdNotReached(
        byte[] thingId, byte[] proposalId, string voteAggIpfsCid, List<ulong> verifiersToSlashIndices
    );
    Task FinalizeAssessmentPollForProposalAsAccepted(
        byte[] thingId, byte[] proposalId, string voteAggIpfsCid, List<ulong> verifiersToSlashIndices
    );
    Task FinalizeAssessmentPollForProposalAsSoftDeclined(
        byte[] thingId, byte[] proposalId, string voteAggIpfsCid, List<ulong> verifiersToSlashIndices
    );
    Task FinalizeAssessmentPollForProposalAsHardDeclined(
        byte[] thingId, byte[] proposalId, string voteAggIpfsCid, List<ulong> verifiersToSlashIndices
    );
    Task<IEnumerable<string>> GetVerifiersForProposal(byte[] thingId, byte[] proposalId);
}
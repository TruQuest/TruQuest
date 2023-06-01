using System.Numerics;

namespace Application.Common.Interfaces;

public interface IContractCaller
{
    Task<byte[]> ComputeHashForThingSubmissionVerifierLottery(byte[] data);
    Task<long> InitThingSubmissionVerifierLottery(byte[] thingId, byte[] dataHash);
    Task<BigInteger> ComputeNonceForThingSubmissionVerifierLottery(byte[] thingId, string accountName, byte[] data);
    Task CloseThingSubmissionVerifierLotteryWithSuccess(byte[] thingId, byte[] data, List<ulong> winnerIndices);
    Task CloseThingSubmissionVerifierLotteryInFailure(byte[] thingId, int joinedNumVerifiers);
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
    Task<long> InitThingAssessmentVerifierLottery(byte[] thingId, byte[] proposalId, byte[] dataHash);
    Task<BigInteger> ComputeNonceForThingAssessmentVerifierLottery(byte[] thingId, byte[] proposalId, string accountName, byte[] data);
    Task<byte[]> ComputeHashForThingAssessmentVerifierLottery(byte[] data);
    Task CloseThingAssessmentVerifierLotteryWithSuccess(
        byte[] thingId, byte[] proposalId, byte[] data, List<ulong> winnerClaimantIndices, List<ulong> winnerIndices
    );
    Task CloseThingAssessmentVerifierLotteryInFailure(byte[] thingId, byte[] proposalId, int joinedNumVerifiers);
    Task<IEnumerable<string>> GetVerifiersForProposal(byte[] thingId, byte[] proposalId);
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
}
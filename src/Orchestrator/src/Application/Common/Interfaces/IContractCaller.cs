using System.Numerics;

using Application.Common.Attributes;
using Application.Ethereum.Common.Models.IM;

namespace Application.Common.Interfaces;

public interface IContractCaller
{
    Task<string> GetWalletAddressFor(string ownerAddress);
    Task<BigInteger> GetWalletNonce(string walletAddress);
    Task<byte[]> GetUserOperationHash(UserOperation userOp);

    Task<int> GetThingValidationVerifierLotteryNumVerifiers();
    Task<int> GetThingValidationVerifierLotteryDurationBlocks();
    Task<IEnumerable<string>> GetThingValidationVerifierLotteryParticipants(byte[] thingId);
    Task<byte[]> ComputeHashForThingValidationVerifierLottery(byte[] data);
    [TrackGasUsage(MetricName = "init-thing-lottery")]
    Task<long> InitThingValidationVerifierLottery(byte[] thingId, byte[] dataHash, byte[] userXorDataHash);
    Task<long> GetThingValidationVerifierLotteryInitBlock(byte[] thingId);
    Task<bool> CheckThingValidationVerifierLotteryExpired(byte[] thingId);
    Task<BigInteger> GetThingValidationVerifierLotteryMaxNonce();
    [TrackGasUsage(MetricName = "close-thing-lottery-success")]
    Task CloseThingValidationVerifierLotteryWithSuccess(
        byte[] thingId, byte[] data, byte[] userXorData, byte[] hashOfL1EndBlock, List<ulong> winnerIndices
    );
    [TrackGasUsage(MetricName = "close-thing-lottery-failure")]
    Task CloseThingValidationVerifierLotteryInFailure(byte[] thingId, int joinedNumVerifiers);

    Task<int> GetThingValidationPollVotingVolumeThresholdPercent();
    Task<int> GetThingValidationPollMajorityThresholdPercent();
    Task<int> GetThingValidationPollDurationBlocks();
    Task<long> GetThingValidationPollInitBlock(byte[] thingId);
    Task FinalizeThingValidationPollAsUnsettledDueToInsufficientVotingVolume(
        byte[] thingId, string voteAggIpfsCid, List<ulong> verifiersToSlashIndices
    );
    Task FinalizeThingValidationPollAsUnsettledDueToMajorityThresholdNotReached(
        byte[] thingId, string voteAggIpfsCid, List<ulong> verifiersToSlashIndices
    );
    Task FinalizeThingValidationPollAsAccepted(
        byte[] thingId, string voteAggIpfsCid, List<ulong> verifiersToSlashIndices
    );
    Task FinalizeThingValidationPollAsSoftDeclined(
        byte[] thingId, string voteAggIpfsCid, List<ulong> verifiersToSlashIndices
    );
    Task FinalizeThingValidationPollAsHardDeclined(
        byte[] thingId, string voteAggIpfsCid, List<ulong> verifiersToSlashIndices
    );
    Task<IEnumerable<string>> GetVerifiersForThing(byte[] thingId);

    Task<int> GetSettlementProposalAssessmentVerifierLotteryNumVerifiers();
    Task<int> GetSettlementProposalAssessmentVerifierLotteryDurationBlocks();
    Task<IEnumerable<string>> GetSettlementProposalAssessmentVerifierLotterySpotClaimants(byte[] thingId, byte[] proposalId);
    Task<IEnumerable<string>> GetSettlementProposalAssessmentVerifierLotteryParticipants(byte[] thingId, byte[] proposalId);
    Task<byte[]> ComputeHashForSettlementProposalAssessmentVerifierLottery(byte[] data);
    Task<long> InitSettlementProposalAssessmentVerifierLottery(byte[] thingId, byte[] proposalId, byte[] dataHash, byte[] userXorDataHash);
    Task<long> GetSettlementProposalAssessmentVerifierLotteryInitBlock(byte[] thingId, byte[] proposalId);
    Task<bool> CheckSettlementProposalAssessmentVerifierLotteryExpired(byte[] thingId, byte[] proposalId);
    Task<BigInteger> GetSettlementProposalAssessmentVerifierLotteryMaxNonce();
    Task CloseSettlementProposalAssessmentVerifierLotteryWithSuccess(
        byte[] thingId, byte[] proposalId, byte[] data, byte[] userXorData,
        byte[] hashOfL1EndBlock, List<ulong> winnerClaimantIndices, List<ulong> winnerIndices
    );
    Task CloseSettlementProposalAssessmentVerifierLotteryInFailure(byte[] thingId, byte[] proposalId, int joinedNumVerifiers);

    Task<int> GetSettlementProposalAssessmentPollVotingVolumeThresholdPercent();
    Task<int> GetSettlementProposalAssessmentPollMajorityThresholdPercent();
    Task<int> GetSettlementProposalAssessmentPollDurationBlocks();
    Task<long> GetSettlementProposalAssessmentPollInitBlock(byte[] thingId, byte[] proposalId);
    Task FinalizeSettlementProposalAssessmentPollAsUnsettledDueToInsufficientVotingVolume(
        byte[] thingId, byte[] proposalId, string voteAggIpfsCid, List<ulong> verifiersToSlashIndices
    );
    Task FinalizeSettlementProposalAssessmentPollAsUnsettledDueToMajorityThresholdNotReached(
        byte[] thingId, byte[] proposalId, string voteAggIpfsCid, List<ulong> verifiersToSlashIndices
    );
    Task FinalizeSettlementProposalAssessmentPollAsAccepted(
        byte[] thingId, byte[] proposalId, string voteAggIpfsCid, List<ulong> verifiersToSlashIndices
    );
    Task FinalizeSettlementProposalAssessmentPollAsSoftDeclined(
        byte[] thingId, byte[] proposalId, string voteAggIpfsCid, List<ulong> verifiersToSlashIndices
    );
    Task FinalizeSettlementProposalAssessmentPollAsHardDeclined(
        byte[] thingId, byte[] proposalId, string voteAggIpfsCid, List<ulong> verifiersToSlashIndices
    );
    Task<IEnumerable<string>> GetVerifiersForSettlementProposal(byte[] thingId, byte[] proposalId);
}

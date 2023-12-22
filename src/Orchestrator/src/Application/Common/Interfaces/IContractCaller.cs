using System.Numerics;

using Application.Common.Attributes;
using Application.Ethereum.Common.Models.IM;
using Application.General.Queries.GetContractsStates.QM;

namespace Application.Common.Interfaces;

public interface IContractCaller
{
    Task<string> GetWalletAddressFor(string ownerAddress);
    Task<BigInteger> GetWalletNonce(string walletAddress);
    Task<byte[]> GetUserOperationHash(UserOperation userOp);

    Task<int> GetThingValidationVerifierLotteryNumVerifiers();
    Task<int> GetThingValidationVerifierLotteryDurationBlocks();
    Task<IEnumerable<string>> GetThingValidationVerifierLotteryParticipants(byte[] thingId);
    [TrackGasUsage(MetricName = "init-thing-lottery")]
    Task<long> InitThingValidationVerifierLottery(byte[] thingId, byte[] dataHash, byte[] userXorDataHash);
    Task<long> GetThingValidationVerifierLotteryInitBlock(byte[] thingId);
    Task<bool> CheckThingValidationVerifierLotteryExpired(byte[] thingId);
    Task<BigInteger> GetThingValidationVerifierLotteryMaxNonce();
    [TrackGasUsage(MetricName = "close-thing-lottery-with-success")]
    Task CloseThingValidationVerifierLotteryWithSuccess(
        byte[] thingId, byte[] data, byte[] userXorData, byte[] hashOfL1EndBlock, List<ulong> winnerIndices
    );
    [TrackGasUsage(MetricName = "close-thing-lottery-in-failure")]
    Task CloseThingValidationVerifierLotteryInFailure(byte[] thingId, int joinedNumVerifiers);

    Task<int> GetThingValidationPollVotingVolumeThresholdPercent();
    Task<int> GetThingValidationPollMajorityThresholdPercent();
    Task<int> GetThingValidationPollDurationBlocks();
    Task<long> GetThingValidationPollInitBlock(byte[] thingId);
    [TrackGasUsage(MetricName = "finalize-thing-poll-as-unsettled")]
    Task FinalizeThingValidationPollAsUnsettledDueToInsufficientVotingVolume(
        byte[] thingId, string voteAggIpfsCid, List<ulong> verifiersToSlashIndices
    );
    [TrackGasUsage(MetricName = "finalize-thing-poll-as-unsettled")]
    Task FinalizeThingValidationPollAsUnsettledDueToMajorityThresholdNotReached(
        byte[] thingId, string voteAggIpfsCid, List<ulong> verifiersToSlashIndices
    );
    [TrackGasUsage(MetricName = "finalize-thing-poll-as-accepted")]
    Task FinalizeThingValidationPollAsAccepted(
        byte[] thingId, string voteAggIpfsCid, List<ulong> verifiersToSlashIndices
    );
    [TrackGasUsage(MetricName = "finalize-thing-poll-as-soft-declined")]
    Task FinalizeThingValidationPollAsSoftDeclined(
        byte[] thingId, string voteAggIpfsCid, List<ulong> verifiersToSlashIndices
    );
    [TrackGasUsage(MetricName = "finalize-thing-poll-as-hard-declined")]
    Task FinalizeThingValidationPollAsHardDeclined(
        byte[] thingId, string voteAggIpfsCid, List<ulong> verifiersToSlashIndices
    );
    Task<IEnumerable<string>> GetVerifiersForThing(byte[] thingId);

    Task<int> GetSettlementProposalAssessmentVerifierLotteryNumVerifiers();
    Task<int> GetSettlementProposalAssessmentVerifierLotteryDurationBlocks();
    Task<IEnumerable<string>> GetSettlementProposalAssessmentVerifierLotterySpotClaimants(byte[] thingId, byte[] proposalId);
    Task<IEnumerable<string>> GetSettlementProposalAssessmentVerifierLotteryParticipants(byte[] thingId, byte[] proposalId);
    [TrackGasUsage(MetricName = "init-proposal-lottery")]
    Task<long> InitSettlementProposalAssessmentVerifierLottery(byte[] thingId, byte[] proposalId, byte[] dataHash, byte[] userXorDataHash);
    Task<long> GetSettlementProposalAssessmentVerifierLotteryInitBlock(byte[] thingId, byte[] proposalId);
    Task<bool> CheckSettlementProposalAssessmentVerifierLotteryExpired(byte[] thingId, byte[] proposalId);
    Task<BigInteger> GetSettlementProposalAssessmentVerifierLotteryMaxNonce();
    [TrackGasUsage(MetricName = "close-proposal-lottery-with-success")]
    Task CloseSettlementProposalAssessmentVerifierLotteryWithSuccess(
        byte[] thingId, byte[] proposalId, byte[] data, byte[] userXorData,
        byte[] hashOfL1EndBlock, List<ulong> winnerClaimantIndices, List<ulong> winnerIndices
    );
    [TrackGasUsage(MetricName = "close-proposal-lottery-in-failure")]
    Task CloseSettlementProposalAssessmentVerifierLotteryInFailure(byte[] thingId, byte[] proposalId, int joinedNumVerifiers);

    Task<int> GetSettlementProposalAssessmentPollVotingVolumeThresholdPercent();
    Task<int> GetSettlementProposalAssessmentPollMajorityThresholdPercent();
    Task<int> GetSettlementProposalAssessmentPollDurationBlocks();
    Task<long> GetSettlementProposalAssessmentPollInitBlock(byte[] thingId, byte[] proposalId);
    [TrackGasUsage(MetricName = "finalize-proposal-poll-as-unsettled")]
    Task FinalizeSettlementProposalAssessmentPollAsUnsettledDueToInsufficientVotingVolume(
        byte[] thingId, byte[] proposalId, string voteAggIpfsCid, List<ulong> verifiersToSlashIndices
    );
    [TrackGasUsage(MetricName = "finalize-proposal-poll-as-unsettled")]
    Task FinalizeSettlementProposalAssessmentPollAsUnsettledDueToMajorityThresholdNotReached(
        byte[] thingId, byte[] proposalId, string voteAggIpfsCid, List<ulong> verifiersToSlashIndices
    );
    [TrackGasUsage(MetricName = "finalize-proposal-poll-as-accepted")]
    Task FinalizeSettlementProposalAssessmentPollAsAccepted(
        byte[] thingId, byte[] proposalId, string voteAggIpfsCid, List<ulong> verifiersToSlashIndices
    );
    [TrackGasUsage(MetricName = "finalize-proposal-poll-as-soft-declined")]
    Task FinalizeSettlementProposalAssessmentPollAsSoftDeclined(
        byte[] thingId, byte[] proposalId, string voteAggIpfsCid, List<ulong> verifiersToSlashIndices
    );
    [TrackGasUsage(MetricName = "finalize-proposal-poll-as-hard-declined")]
    Task FinalizeSettlementProposalAssessmentPollAsHardDeclined(
        byte[] thingId, byte[] proposalId, string voteAggIpfsCid, List<ulong> verifiersToSlashIndices
    );
    Task<IEnumerable<string>> GetVerifiersForSettlementProposal(byte[] thingId, byte[] proposalId);

    Task<TruQuestContractInfoQm> ExportTruQuestContractInfo();
    Task<List<string>> GetRestrictedAccessWhitelist();
    Task<IEnumerable<UserBalanceQm>> ExportUsersAndBalances();
    Task<IEnumerable<ThingSubmitterQm>> ExportThingSubmitter();
    Task<IEnumerable<SettlementProposalSubmitterQm>> ExportThingIdToSettlementProposal();
    Task<IEnumerable<ThingValidationVerifierLotteryQm>> ExportThingValidationVerifierLotteryData();
    Task<IEnumerable<ThingValidationPollQm>> ExportThingValidationPollData();
    Task<IEnumerable<SettlementProposalAssessmentVerifierLotteryQm>> ExportSettlementProposalAssessmentVerifierLotteryData();
    Task<IEnumerable<SettlementProposalAssessmentPollQm>> ExportSettlementProposalAssessmentPollData();
}

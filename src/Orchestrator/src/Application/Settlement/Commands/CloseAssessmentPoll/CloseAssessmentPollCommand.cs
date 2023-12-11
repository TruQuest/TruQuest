using System.Diagnostics;

using Microsoft.Extensions.Logging;

using GoThataway;

using Domain.Aggregates;
using Domain.Aggregates.Events;
using Domain.Results;

using Application.Common.Interfaces;
using Application.Common.Misc;
using Application.Common.Models.IM;
using Application.Common.Monitoring;
using static Application.Common.Monitoring.LogMessagePlaceholders;

namespace Application.Settlement.Commands.CloseAssessmentPoll;

public class CloseAssessmentPollCommand : DeferredTaskCommand, IRequest<VoidResult>
{
    public required Guid ThingId { get; init; }
    public required Guid SettlementProposalId { get; init; }
    public required long EndBlock { get; init; }
    public required long TaskId { get; init; }

    public IEnumerable<(string Name, object? Value)> GetActivityTags(VoidResult _)
    {
        return new (string, object?)[]
        {
            (ActivityTags.ThingId, ThingId),
            (ActivityTags.SettlementProposalId, SettlementProposalId),
            (ActivityTags.EndBlockNum, EndBlock),
            (ActivityTags.TaskId, TaskId)
        };
    }
}

public class CloseAssessmentPollCommandHandler : IRequestHandler<CloseAssessmentPollCommand, VoidResult>
{
    private readonly ILogger<CloseAssessmentPollCommandHandler> _logger;
    private readonly ITaskRepository _taskRepository;
    private readonly IL1BlockchainQueryable _blockchainQueryable;
    private readonly ISettlementProposalAssessmentPollVoteRepository _voteRepository;
    private readonly ICastedSettlementProposalAssessmentPollVoteEventRepository _castedAssessmentPollVoteEventRepository;
    private readonly ISigner _signer;
    private readonly IFileStorage _fileStorage;
    private readonly IContractCaller _contractCaller;

    public CloseAssessmentPollCommandHandler(
        ILogger<CloseAssessmentPollCommandHandler> logger,
        ITaskRepository taskRepository,
        IL1BlockchainQueryable blockchainQueryable,
        ISettlementProposalAssessmentPollVoteRepository voteRepository,
        ICastedSettlementProposalAssessmentPollVoteEventRepository castedAssessmentPollVoteEventRepository,
        ISigner signer,
        IFileStorage fileStorage,
        IContractCaller contractCaller
    )
    {
        _logger = logger;
        _taskRepository = taskRepository;
        _blockchainQueryable = blockchainQueryable;
        _voteRepository = voteRepository;
        _castedAssessmentPollVoteEventRepository = castedAssessmentPollVoteEventRepository;
        _signer = signer;
        _fileStorage = fileStorage;
        _contractCaller = contractCaller;
    }

    public async Task<VoidResult> Handle(CloseAssessmentPollCommand command, CancellationToken ct)
    {
        long upperLimitTs = await _blockchainQueryable.GetBlockTimestamp(command.EndBlock);

        // @@TODO: Use queryable instead of repo.
        var offChainVotes = await _voteRepository.GetFor(command.SettlementProposalId);

        // @@TODO: Use queryable instead of repo.
        var castedVoteEvents = await _castedAssessmentPollVoteEventRepository.GetAllFor(
            command.ThingId, command.SettlementProposalId
        );

        var orchestratorSignature = _signer.SignSettlementProposalAssessmentPollVoteAgg(
            command.ThingId, command.SettlementProposalId,
            (ulong)command.EndBlock, offChainVotes, castedVoteEvents
        );

        var result = await _fileStorage.UploadJson(new
        {
            command.ThingId,
            command.SettlementProposalId,
            L1EndBlock = command.EndBlock,
            OffChainVotes = offChainVotes
                .Select(v => new
                {
                    v.IpfsCid
                }),
            OnChainVotes = castedVoteEvents
                .Select(v => new
                {
                    v.TxnHash,
                    v.BlockNumber,
                    v.TxnIndex,
                    v.LogIndex,
                    UserId = v.UserId!,
                    v.WalletAddress,
                    Decision = v.Decision.GetString(),
                    Reason = v.Reason ?? string.Empty
                }),
            OrchestratorSignature = orchestratorSignature
        });

        if (result.IsError)
        {
            _logger.LogWarning(
                $"Error trying to upload settlement proposal {SettlementProposalId} assessment poll's aggregate vote to IPFS: {result.Error}",
                command.SettlementProposalId
            );
            return new()
            {
                Error = result.Error
            };
        }

        var accountedVotes = new HashSet<AccountedVote>();
        foreach (var onChainVote in
            castedVoteEvents
                .OrderByDescending(e => e.BlockNumber)
                    .ThenByDescending(e => e.TxnIndex)
                       .ThenByDescending(e => e.LogIndex)
        )
        {
            accountedVotes.Add(new()
            {
                VoterId = onChainVote.UserId!,
                VoterWalletAddress = onChainVote.WalletAddress,
                VoteDecision = (AccountedVote.Decision)onChainVote.Decision
            });
        }
        foreach (var offChainVote in
            offChainVotes
                .Where(v => v.CastedAtMs <= upperLimitTs)
                .OrderByDescending(v => v.CastedAtMs)
        )
        {
            accountedVotes.Add(new()
            {
                VoterId = offChainVote.VoterId,
                VoterWalletAddress = offChainVote.VoterWalletAddress,
                VoteDecision = (AccountedVote.Decision)offChainVote.Decision
            });
        }

        var verifierAddresses = await _contractCaller.GetVerifiersForSettlementProposal(
            command.ThingId.ToByteArray(),
            command.SettlementProposalId.ToByteArray()
        );
        var verifierAddressesIndexed = verifierAddresses
            .Select((address, i) => (Address: address, Index: i))
            .ToList();

        var notVotedVerifierIndices = verifierAddressesIndexed
            .Where(vi => accountedVotes.SingleOrDefault(v => v.VoterWalletAddress == vi.Address) == null)
            .Select(vi => (ulong)vi.Index)
            .ToList();

        int votingVolumeThresholdPercent = await _contractCaller.GetSettlementProposalAssessmentPollVotingVolumeThresholdPercent();

        var requiredVoterCount = Math.Ceiling(votingVolumeThresholdPercent / 100f * verifierAddresses.Count());
        if (accountedVotes.Count < requiredVoterCount)
        {
            await _contractCaller.FinalizeSettlementProposalAssessmentPollAsUnsettledDueToInsufficientVotingVolume(
                command.ThingId.ToByteArray(), command.SettlementProposalId.ToByteArray(),
                result.Data!, notVotedVerifierIndices
            );

            _logger.LogInformation(
                $"Finalized settlement proposal {SettlementProposalId} assessment poll as unsettled: insufficient voting volume.\n" +
                $"Required at least {RequiredVoterCount} voters out of {NumVerifiers} to vote, but got {ActualVoterCount}",
                command.SettlementProposalId, requiredVoterCount, verifierAddresses.Count(), accountedVotes.Count
            );

            return VoidResult.Instance;
        }

        Debug.Assert(accountedVotes.Count > 0);

        int majorityThresholdPercent = await _contractCaller.GetSettlementProposalAssessmentPollMajorityThresholdPercent();
        Debug.Assert(majorityThresholdPercent >= 51);
        var acceptedDecisionRequiredVoteCount = Math.Ceiling(majorityThresholdPercent / 100f * accountedVotes.Count);

        var votesGroupedByDecision = accountedVotes.GroupBy(v => v.VoteDecision);
        Debug.Assert(accountedVotes.Count == votesGroupedByDecision.Aggregate(0, (count, group) => count + group.Count()));
        var acceptedDecision = votesGroupedByDecision.MaxBy(group => group.Count())!;

        if (acceptedDecision.Count() < acceptedDecisionRequiredVoteCount)
        {
            await _contractCaller.FinalizeSettlementProposalAssessmentPollAsUnsettledDueToMajorityThresholdNotReached(
                command.ThingId.ToByteArray(), command.SettlementProposalId.ToByteArray(),
                result.Data!, notVotedVerifierIndices
            );

            _logger.LogInformation(
                $"Finalized settlement proposal {SettlementProposalId} assessment poll as unsettled: majority threshold not reached.\n" +
                $"Required at least {RequiredVoteCount} votes out of {TotalVoteCount} to be in concord, but got {ActualVoteCount} at most",
                command.SettlementProposalId, acceptedDecisionRequiredVoteCount, accountedVotes.Count, acceptedDecision.Count()
            );

            return VoidResult.Instance;
        }

        var verifiersThatDisagreedWithAcceptedDecisionDirection = votesGroupedByDecision
            .Where(v => v.Key.GetScore() != acceptedDecision.Key.GetScore())
            .SelectMany(v => v)
            .Select(v => v.VoterWalletAddress)
            .ToList();

        var indicesOfVerifiersThatDisagreedWithAcceptedDecisionDirection = verifierAddressesIndexed
            .Where(vi => verifiersThatDisagreedWithAcceptedDecisionDirection
                .SingleOrDefault(verifier => verifier == vi.Address) != null
            )
            .Select(vi => (ulong)vi.Index);

        var verifiersToSlashIndices = notVotedVerifierIndices
            .Concat(indicesOfVerifiersThatDisagreedWithAcceptedDecisionDirection)
            .Order()
            .ToList();

        if (acceptedDecision.Key == AccountedVote.Decision.Accept)
        {
            await _contractCaller.FinalizeSettlementProposalAssessmentPollAsAccepted(
                thingId: command.ThingId.ToByteArray(),
                proposalId: command.SettlementProposalId.ToByteArray(),
                voteAggIpfsCid: result.Data!,
                verifiersToSlashIndices: verifiersToSlashIndices
            );
        }
        else if (acceptedDecision.Key == AccountedVote.Decision.SoftDecline)
        {
            await _contractCaller.FinalizeSettlementProposalAssessmentPollAsSoftDeclined(
                thingId: command.ThingId.ToByteArray(),
                proposalId: command.SettlementProposalId.ToByteArray(),
                voteAggIpfsCid: result.Data!,
                verifiersToSlashIndices: verifiersToSlashIndices
            );
        }
        else
        {
            await _contractCaller.FinalizeSettlementProposalAssessmentPollAsHardDeclined(
                thingId: command.ThingId.ToByteArray(),
                proposalId: command.SettlementProposalId.ToByteArray(),
                voteAggIpfsCid: result.Data!,
                verifiersToSlashIndices: verifiersToSlashIndices
            );
        }

        await _taskRepository.SetCompletedStateFor(command.TaskId);

        _logger.LogInformation(
            $"Finalized settlement proposal {SettlementProposalId} assessment poll. Decision: {Decision}.\n" +
            $"Accept: {NumAcceptVotes} vote(s)\n" +
            $"Soft Decline: {NumSoftDeclineVotes} vote(s)\n" +
            $"Hard Decline: {NumHardDeclineVotes} vote(s)",
            command.SettlementProposalId, acceptedDecision.Key,
            votesGroupedByDecision.SingleOrDefault(v => v.Key == AccountedVote.Decision.Accept)?.Count() ?? 0,
            votesGroupedByDecision.SingleOrDefault(v => v.Key == AccountedVote.Decision.SoftDecline)?.Count() ?? 0,
            votesGroupedByDecision.SingleOrDefault(v => v.Key == AccountedVote.Decision.HardDecline)?.Count() ?? 0
        );

        return VoidResult.Instance;
    }
}

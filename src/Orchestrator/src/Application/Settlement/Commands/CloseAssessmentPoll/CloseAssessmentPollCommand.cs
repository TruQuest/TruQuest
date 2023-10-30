using System.Diagnostics;

using Microsoft.Extensions.Logging;

using MediatR;

using Domain.Aggregates;
using Domain.Aggregates.Events;
using Domain.Results;

using Application.Common.Interfaces;
using Application.Common.Misc;
using Application.Common.Models.IM;

namespace Application.Settlement.Commands.CloseAssessmentPoll;

internal class CloseAssessmentPollCommand : DeferredTaskCommand, IRequest<VoidResult>
{
    public required Guid ThingId { get; init; }
    public required Guid SettlementProposalId { get; init; }
    public required long EndBlock { get; init; }
    public required long TaskId { get; init; }
}

internal class CloseAssessmentPollCommandHandler : IRequestHandler<CloseAssessmentPollCommand, VoidResult>
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
                    UserId = v.UserId!,
                    v.WalletAddress,
                    Decision = v.Decision.GetString(),
                    Reason = v.Reason ?? string.Empty
                }),
            OrchestratorSignature = orchestratorSignature
        });

        if (result.IsError)
        {
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
        _logger.LogDebug("Proposal {ProposalId} Poll: {NumVerifiers} verifiers", command.SettlementProposalId, verifierAddresses.Count());

        var notVotedVerifierIndices = verifierAddressesIndexed
            .Where(vi => accountedVotes.SingleOrDefault(v => v.VoterWalletAddress == vi.Address) == null)
            .Select(vi => (ulong)vi.Index)
            .ToList();

        _logger.LogDebug(
            "Proposal {ProposalId} Poll: {NumVerifiers} not voted",
            command.SettlementProposalId, notVotedVerifierIndices.Count
        );

        int votingVolumeThresholdPercent = await _contractCaller.GetSettlementProposalAssessmentPollVotingVolumeThresholdPercent();

        var requiredVoterCount = Math.Ceiling(votingVolumeThresholdPercent / 100f * verifierAddresses.Count());
        _logger.LogDebug("Required voter count: {VoterCount}", requiredVoterCount);

        if (accountedVotes.Count < requiredVoterCount)
        {
            _logger.LogInformation(
                "Proposal {ProposalId} Lottery: Insufficient voting volume. " +
                "Required at least {RequiredVoterCount} voters out of {NumVerifiers} to vote; Got {ActualVoterCount}",
                command.SettlementProposalId, requiredVoterCount, verifierAddresses.Count(), accountedVotes.Count
            );
            await _contractCaller.FinalizeSettlementProposalAssessmentPollAsUnsettledDueToInsufficientVotingVolume(
                command.ThingId.ToByteArray(), command.SettlementProposalId.ToByteArray(),
                result.Data!, notVotedVerifierIndices
            );

            return VoidResult.Instance;
        }

        Debug.Assert(accountedVotes.Count > 0);

        int majorityThresholdPercent = await _contractCaller.GetSettlementProposalAssessmentPollMajorityThresholdPercent();
        Debug.Assert(majorityThresholdPercent >= 51);
        var acceptedDecisionRequiredVoteCount = Math.Ceiling(majorityThresholdPercent / 100f * accountedVotes.Count);
        _logger.LogDebug("Accepted decision required vote count: {VoteCount}", acceptedDecisionRequiredVoteCount);

        var votesGroupedByDecision = accountedVotes.GroupBy(v => v.VoteDecision);
        Debug.Assert(accountedVotes.Count == votesGroupedByDecision.Aggregate(0, (count, group) => count + group.Count()));
        var acceptedDecision = votesGroupedByDecision.MaxBy(group => group.Count())!;

        if (acceptedDecision.Count() < acceptedDecisionRequiredVoteCount)
        {
            _logger.LogInformation(
                "Proposal {ProposalId} Lottery: Majority threshold not reached. " +
                "Required at least {RequiredVoteCount} votes out of {TotalVoteCount}; Got {ActualVoteCount}",
                command.SettlementProposalId, acceptedDecisionRequiredVoteCount, accountedVotes.Count, acceptedDecision.Count()
            );
            await _contractCaller.FinalizeSettlementProposalAssessmentPollAsUnsettledDueToMajorityThresholdNotReached(
                command.ThingId.ToByteArray(), command.SettlementProposalId.ToByteArray(),
                result.Data!, notVotedVerifierIndices
            );

            return VoidResult.Instance;
        }

        var verifiersThatDisagreedWithAcceptedDecisionDirection = votesGroupedByDecision
            .Where(v => v.Key.GetScore() != acceptedDecision.Key.GetScore())
            .SelectMany(v => v)
            .Select(v => v.VoterWalletAddress)
            .ToList();

        _logger.LogInformation(
            "Proposal {ProposalId} Lottery Decision: {Decision}.\n" +
            "Accept: {NumAcceptedVotes} votes.\n" +
            "Soft Decline: {NumSoftDeclinedVotes} votes.\n" +
            "Hard Decline: {NumHardDeclinedVotes} votes.",
            command.SettlementProposalId, acceptedDecision.Key,
            votesGroupedByDecision.SingleOrDefault(v => v.Key == AccountedVote.Decision.Accept)?.Count() ?? 0,
            votesGroupedByDecision.SingleOrDefault(v => v.Key == AccountedVote.Decision.SoftDecline)?.Count() ?? 0,
            votesGroupedByDecision.SingleOrDefault(v => v.Key == AccountedVote.Decision.HardDecline)?.Count() ?? 0
        );

        var indicesOfVerifiersThatDisagreedWithAcceptedDecisionDirection = verifierAddressesIndexed
            .Where(vi => verifiersThatDisagreedWithAcceptedDecisionDirection
                .SingleOrDefault(verifier => verifier == vi.Address) != null
            )
            .Select(vi => (ulong)vi.Index);

        var verifiersToSlashIndices = notVotedVerifierIndices
            .Concat(indicesOfVerifiersThatDisagreedWithAcceptedDecisionDirection)
            .Order()
            .ToList();

        _logger.LogInformation(
            "Proposal {ProposalId} Lottery: Slashing {VerifierCount} verifiers...",
            command.SettlementProposalId, verifiersToSlashIndices.Count
        );

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

        return VoidResult.Instance;
    }
}

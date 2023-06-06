using System.Diagnostics;

using Microsoft.Extensions.Logging;

using MediatR;

using Domain.Aggregates;
using Domain.Aggregates.Events;
using Domain.Results;

using Application.Common.Interfaces;
using Application.Common.Misc;

namespace Application.Settlement.Commands.CloseAssessmentPoll;

internal class CloseAssessmentPollCommand : IRequest<VoidResult>
{
    public required long LatestIncludedBlockNumber { get; init; }
    public required Guid ThingId { get; init; }
    public required Guid SettlementProposalId { get; init; }
}

internal class CloseAssessmentPollCommandHandler : IRequestHandler<CloseAssessmentPollCommand, VoidResult>
{
    private readonly ILogger<CloseAssessmentPollCommandHandler> _logger;
    private readonly IL1BlockchainQueryable _blockchainQueryable;
    private readonly IAssessmentPollVoteRepository _voteRepository;
    private readonly ICastedAssessmentPollVoteEventRepository _castedAssessmentPollVoteEventRepository;
    private readonly ISigner _signer;
    private readonly IFileStorage _fileStorage;
    private readonly IContractCaller _contractCaller;
    private readonly IContractStorageQueryable _contractStorageQueryable;

    public CloseAssessmentPollCommandHandler(
        ILogger<CloseAssessmentPollCommandHandler> logger,
        IL1BlockchainQueryable blockchainQueryable,
        IAssessmentPollVoteRepository voteRepository,
        ICastedAssessmentPollVoteEventRepository castedAssessmentPollVoteEventRepository,
        ISigner signer,
        IFileStorage fileStorage,
        IContractCaller contractCaller,
        IContractStorageQueryable contractStorageQueryable
    )
    {
        _logger = logger;
        _blockchainQueryable = blockchainQueryable;
        _voteRepository = voteRepository;
        _castedAssessmentPollVoteEventRepository = castedAssessmentPollVoteEventRepository;
        _signer = signer;
        _fileStorage = fileStorage;
        _contractCaller = contractCaller;
        _contractStorageQueryable = contractStorageQueryable;
    }

    public async Task<VoidResult> Handle(CloseAssessmentPollCommand command, CancellationToken ct)
    {
        long upperLimitTs = await _blockchainQueryable.GetBlockTimestamp(command.LatestIncludedBlockNumber);

        // @@TODO: Use queryable instead of repo.
        var offChainVotes = await _voteRepository.GetFor(command.SettlementProposalId);

        // @@TODO: Use queryable instead of repo.
        var castedVoteEvents = await _castedAssessmentPollVoteEventRepository.GetAllFor(
            command.ThingId, command.SettlementProposalId
        );

        var orchestratorSig = _signer.SignAssessmentPollVoteAgg(
            command.ThingId, command.SettlementProposalId,
            (ulong)command.LatestIncludedBlockNumber, offChainVotes, castedVoteEvents
        );

        var result = await _fileStorage.UploadJson(new
        {
            command.ThingId,
            command.SettlementProposalId,
            EndBlock = command.LatestIncludedBlockNumber,
            OffChainVotes = offChainVotes
                .Select(v => new
                {
                    IpfsCid = v.IpfsCid
                }),
            OnChainVotes = castedVoteEvents
                .Select(v => new
                {
                    BlockNumber = v.BlockNumber,
                    TxnIndex = v.TxnIndex,
                    UserId = v.UserId,
                    Decision = v.Decision.GetString(),
                    Reason = v.Reason ?? string.Empty
                }),
            OrchestratorSignature = orchestratorSig
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
                VoterId = onChainVote.UserId,
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
                VoteDecision = (AccountedVote.Decision)offChainVote.Decision
            });
        }

        var verifiers = await _contractCaller.GetVerifiersForProposal(
            command.ThingId.ToByteArray(),
            command.SettlementProposalId.ToByteArray()
        );
        var verifiersIndexed = verifiers
            .Select((verifier, i) => (VerifierId: verifier.Substring(2).ToLower(), Index: i))
            .ToList();
        _logger.LogDebug("Proposal {ProposalId} Poll: {NumVerifiers} verifiers", command.SettlementProposalId, verifiers.Count());

        var notVotedVerifierIndices = verifiersIndexed
            .Where(vi => accountedVotes.SingleOrDefault(v => v.VoterId == vi.VerifierId) == null)
            .Select(vi => (ulong)vi.Index)
            .ToList();

        _logger.LogDebug(
            "Proposal {ProposalId} Poll: {NumVerifiers} not voted",
            command.SettlementProposalId, notVotedVerifierIndices.Count
        );

        int votingVolumeThresholdPercent = await _contractStorageQueryable.GetThingAssessmentPollVotingVolumeThreshold();

        var requiredVoterCount = Math.Ceiling(votingVolumeThresholdPercent / 100f * verifiers.Count());
        _logger.LogDebug("Required voter count: {VoterCount}", requiredVoterCount);

        if (accountedVotes.Count < requiredVoterCount)
        {
            _logger.LogInformation(
                "Proposal {ProposalId} Lottery: Insufficient voting volume. " +
                "Required at least {RequiredVoterCount} voters out of {NumVerifiers} to vote; Got {ActualVoterCount}",
                command.SettlementProposalId, requiredVoterCount, verifiers.Count(), accountedVotes.Count
            );
            await _contractCaller.FinalizeAssessmentPollForProposalAsUnsettledDueToInsufficientVotingVolume(
                command.ThingId.ToByteArray(), command.SettlementProposalId.ToByteArray(),
                result.Data!, notVotedVerifierIndices
            );

            return VoidResult.Instance;
        }

        Debug.Assert(accountedVotes.Count > 0);

        int majorityThresholdPercent = await _contractStorageQueryable.GetThingAssessmentPollMajorityThreshold();
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
            await _contractCaller.FinalizeAssessmentPollForProposalAsUnsettledDueToMajorityThresholdNotReached(
                command.ThingId.ToByteArray(), command.SettlementProposalId.ToByteArray(),
                result.Data!, notVotedVerifierIndices
            );

            return VoidResult.Instance;
        }

        var verifiersThatDisagreedWithAcceptedDecisionDirection = votesGroupedByDecision
            .Where(v => v.Key.GetScore() != acceptedDecision.Key.GetScore())
            .SelectMany(v => v)
            .Select(v => v.VoterId)
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

        var indicesOfVerifiersThatDisagreedWithAcceptedDecisionDirection = verifiersIndexed
            .Where(vi => verifiersThatDisagreedWithAcceptedDecisionDirection
                .SingleOrDefault(verifierId => verifierId == vi.VerifierId) != null
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
            await _contractCaller.FinalizeAssessmentPollForProposalAsAccepted(
                thingId: command.ThingId.ToByteArray(),
                proposalId: command.SettlementProposalId.ToByteArray(),
                voteAggIpfsCid: result.Data!,
                verifiersToSlashIndices: verifiersToSlashIndices
            );
        }
        else if (acceptedDecision.Key == AccountedVote.Decision.SoftDecline)
        {
            await _contractCaller.FinalizeAssessmentPollForProposalAsSoftDeclined(
                thingId: command.ThingId.ToByteArray(),
                proposalId: command.SettlementProposalId.ToByteArray(),
                voteAggIpfsCid: result.Data!,
                verifiersToSlashIndices: verifiersToSlashIndices
            );
        }
        else
        {
            await _contractCaller.FinalizeAssessmentPollForProposalAsHardDeclined(
                thingId: command.ThingId.ToByteArray(),
                proposalId: command.SettlementProposalId.ToByteArray(),
                voteAggIpfsCid: result.Data!,
                verifiersToSlashIndices: verifiersToSlashIndices
            );
        }

        return VoidResult.Instance;
    }
}
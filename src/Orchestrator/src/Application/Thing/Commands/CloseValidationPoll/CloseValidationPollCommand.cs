using System.Diagnostics;

using Microsoft.Extensions.Logging;

using GoThataway;

using Domain.Results;
using Domain.Aggregates;
using Domain.Aggregates.Events;

using Application.Common.Interfaces;
using Application.Thing.Commands.CastValidationPollVote;
using Application.Common.Misc;
using Application.Common.Models.IM;
using Application.Common.Monitoring;
using static Application.Common.Monitoring.LogMessagePlaceholders;

namespace Application.Thing.Commands.CloseValidationPoll;

public class CloseValidationPollCommand : DeferredTaskCommand, IRequest<VoidResult>
{
    public required Guid ThingId { get; init; }
    public required long EndBlock { get; init; }
    public required long TaskId { get; init; }

    public IEnumerable<(string Name, object? Value)> GetActivityTags(VoidResult _)
    {
        return new (string, object?)[]
        {
            (ActivityTags.ThingId, ThingId),
            (ActivityTags.EndBlockNum, EndBlock),
            (ActivityTags.TaskId, TaskId)
        };
    }
}

public class CloseValidationPollCommandHandler : IRequestHandler<CloseValidationPollCommand, VoidResult>
{
    private readonly ILogger<CloseValidationPollCommandHandler> _logger;
    private readonly ITaskRepository _taskRepository;
    private readonly IL1BlockchainQueryable _l1BlockchainQueryable;
    private readonly IThingValidationPollVoteRepository _voteRepository;
    private readonly ICastedThingValidationPollVoteEventRepository _castedValidationPollVoteEventRepository;
    private readonly ISigner _signer;
    private readonly IFileStorage _fileStorage;
    private readonly IContractCaller _contractCaller;

    public CloseValidationPollCommandHandler(
        ILogger<CloseValidationPollCommandHandler> logger,
        ITaskRepository taskRepository,
        IL1BlockchainQueryable l1BlockchainQueryable,
        IThingValidationPollVoteRepository voteRepository,
        ICastedThingValidationPollVoteEventRepository castedValidationPollVoteEventRepository,
        ISigner signer,
        IFileStorage fileStorage,
        IContractCaller contractCaller
    )
    {
        _logger = logger;
        _taskRepository = taskRepository;
        _l1BlockchainQueryable = l1BlockchainQueryable;
        _voteRepository = voteRepository;
        _castedValidationPollVoteEventRepository = castedValidationPollVoteEventRepository;
        _signer = signer;
        _fileStorage = fileStorage;
        _contractCaller = contractCaller;
    }

    public async Task<VoidResult> Handle(CloseValidationPollCommand command, CancellationToken ct)
    {
        long upperLimitTs = await _l1BlockchainQueryable.GetBlockTimestamp(command.EndBlock);

        // @@TODO: Use queryable instead of repo.
        var offChainVotes = await _voteRepository.GetFor(command.ThingId);

        // @@TODO: Use queryable instead of repo.
        var castedVoteEvents = await _castedValidationPollVoteEventRepository.GetAllFor(command.ThingId);

        var orchestratorSignature = _signer.SignThingValidationPollVoteAgg(
            command.ThingId, (ulong)command.EndBlock, offChainVotes, castedVoteEvents
        );

        var result = await _fileStorage.UploadJson(new
        {
            command.ThingId,
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
            _logger.LogWarning($"Error trying to upload thing {ThingId} validation poll's aggregate vote to IPFS: {result.Error}", command.ThingId);
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

        var verifierAddresses = await _contractCaller.GetVerifiersForThing(command.ThingId.ToByteArray());
        var verifierAddressesIndexed = verifierAddresses
            .Select((address, i) => (Address: address, Index: i))
            .ToList();

        var notVotedVerifierIndices = verifierAddressesIndexed
            .Where(vi => accountedVotes.SingleOrDefault(v => v.VoterWalletAddress == vi.Address) == null)
            .Select(vi => (ulong)vi.Index)
            .ToList();

        int votingVolumeThresholdPercent = await _contractCaller.GetThingValidationPollVotingVolumeThresholdPercent();

        var requiredVoterCount = Math.Ceiling(votingVolumeThresholdPercent / 100f * verifierAddresses.Count());
        if (accountedVotes.Count < requiredVoterCount)
        {
            await _contractCaller.FinalizeThingValidationPollAsUnsettledDueToInsufficientVotingVolume(
                command.ThingId.ToByteArray(), result.Data!, notVotedVerifierIndices
            );

            _logger.LogInformation(
                $"Finalized thing {ThingId} validation poll as unsettled: insufficient voting volume.\n" +
                $"Required at least {RequiredVoterCount} voters out of {NumVerifiers} to vote, but got {ActualVoterCount}",
                command.ThingId, requiredVoterCount, verifierAddresses.Count(), accountedVotes.Count
            );

            return VoidResult.Instance;
        }

        Debug.Assert(accountedVotes.Count > 0);

        int majorityThresholdPercent = await _contractCaller.GetThingValidationPollMajorityThresholdPercent();
        Debug.Assert(majorityThresholdPercent >= 51);
        var acceptedDecisionRequiredVoteCount = Math.Ceiling(majorityThresholdPercent / 100f * accountedVotes.Count);

        var votesGroupedByDecision = accountedVotes.GroupBy(v => v.VoteDecision);
        Debug.Assert(accountedVotes.Count == votesGroupedByDecision.Aggregate(0, (count, group) => count + group.Count()));
        var acceptedDecision = votesGroupedByDecision.MaxBy(group => group.Count())!;

        if (acceptedDecision.Count() < acceptedDecisionRequiredVoteCount)
        {
            await _contractCaller.FinalizeThingValidationPollAsUnsettledDueToMajorityThresholdNotReached(
                command.ThingId.ToByteArray(), result.Data!, notVotedVerifierIndices
            );

            _logger.LogInformation(
                $"Finalized thing {ThingId} validaton poll as unsettled: majority threshold not reached.\n" +
                $"Required at least {RequiredVoteCount} votes out of {TotalVoteCount} to be in concord, but got {ActualVoteCount} at most",
                command.ThingId, acceptedDecisionRequiredVoteCount, accountedVotes.Count, acceptedDecision.Count()
            );

            return VoidResult.Instance;
        }

        // @@NOTE: Since majorityThresholdPercent >= 51 and acceptedDecision.Count() >= acceptedDecisionRequiredVoteCount,
        // any other decisions could have only received fewer number of votes.

        // @@??: Should the not voted verifiers actually be slashed more than those who voted against the majority?

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
            await _contractCaller.FinalizeThingValidationPollAsAccepted(
                thingId: command.ThingId.ToByteArray(),
                voteAggIpfsCid: result.Data!,
                verifiersToSlashIndices: verifiersToSlashIndices
            );
        }
        // @@??: Would it make sense to reward only those that soft declined specifically, instead
        // of rewarding all who just generally declined?
        else if (acceptedDecision.Key == AccountedVote.Decision.SoftDecline)
        {
            await _contractCaller.FinalizeThingValidationPollAsSoftDeclined(
                thingId: command.ThingId.ToByteArray(),
                voteAggIpfsCid: result.Data!,
                verifiersToSlashIndices: verifiersToSlashIndices
            );
        }
        else
        {
            await _contractCaller.FinalizeThingValidationPollAsHardDeclined(
                thingId: command.ThingId.ToByteArray(),
                voteAggIpfsCid: result.Data!,
                verifiersToSlashIndices: verifiersToSlashIndices
            );
        }

        await _taskRepository.SetCompletedStateFor(command.TaskId);

        _logger.LogInformation(
            $"Finalized thing {ThingId} validation poll. Decision: {Decision}.\n" +
            $"Accept: {NumAcceptVotes} vote(s)\n" +
            $"Soft Decline: {NumSoftDeclineVotes} vote(s)\n" +
            $"Hard Decline: {NumHardDeclineVotes} vote(s)",
            command.ThingId, acceptedDecision.Key,
            votesGroupedByDecision.SingleOrDefault(v => v.Key == AccountedVote.Decision.Accept)?.Count() ?? 0,
            votesGroupedByDecision.SingleOrDefault(v => v.Key == AccountedVote.Decision.SoftDecline)?.Count() ?? 0,
            votesGroupedByDecision.SingleOrDefault(v => v.Key == AccountedVote.Decision.HardDecline)?.Count() ?? 0
        );

        return VoidResult.Instance;
    }
}

using System.Diagnostics;

using Microsoft.Extensions.Logging;

using MediatR;

using Domain.Results;
using Domain.Aggregates;
using Domain.Aggregates.Events;

using Application.Common.Interfaces;
using Application.Thing.Commands.CastAcceptancePollVote;
using Application.Common.Misc;

namespace Application.Thing.Commands.CloseAcceptancePoll;

internal class CloseAcceptancePollCommand : IRequest<VoidResult>
{
    public required long LatestIncludedBlockNumber { get; init; }
    public required Guid ThingId { get; init; }
}

internal class CloseAcceptancePollCommandHandler : IRequestHandler<CloseAcceptancePollCommand, VoidResult>
{
    private readonly ILogger<CloseAcceptancePollCommandHandler> _logger;
    private readonly IBlockchainQueryable _blockchainQueryable;
    private readonly IAcceptancePollVoteRepository _voteRepository;
    private readonly ICastedAcceptancePollVoteEventRepository _castedAcceptancePollVoteEventRepository;
    private readonly IContractStorageQueryable _contractStorageQueryable;
    private readonly IThingRepository _thingRepository;
    private readonly ISigner _signer;
    private readonly IFileStorage _fileStorage;
    private readonly IContractCaller _contractCaller;

    public CloseAcceptancePollCommandHandler(
        ILogger<CloseAcceptancePollCommandHandler> logger,
        IBlockchainQueryable blockchainQueryable,
        IAcceptancePollVoteRepository voteRepository,
        ICastedAcceptancePollVoteEventRepository castedAcceptancePollVoteEventRepository,
        IContractStorageQueryable contractStorageQueryable,
        IThingRepository thingRepository,
        ISigner signer,
        IFileStorage fileStorage,
        IContractCaller contractCaller
    )
    {
        _logger = logger;
        _blockchainQueryable = blockchainQueryable;
        _voteRepository = voteRepository;
        _castedAcceptancePollVoteEventRepository = castedAcceptancePollVoteEventRepository;
        _contractStorageQueryable = contractStorageQueryable;
        _thingRepository = thingRepository;
        _signer = signer;
        _fileStorage = fileStorage;
        _contractCaller = contractCaller;
    }

    public async Task<VoidResult> Handle(CloseAcceptancePollCommand command, CancellationToken ct)
    {
        long upperLimitTs = await _blockchainQueryable.GetBlockTimestamp(command.LatestIncludedBlockNumber);

        // @@TODO: Use queryable instead of repo.
        var offChainVotes = await _voteRepository.GetForThingCastedAt(
            command.ThingId,
            noLaterThanTs: upperLimitTs
        );

        // @@TODO: Use queryable instead of repo.
        var castedVoteEvents = await _castedAcceptancePollVoteEventRepository.GetAllFor(command.ThingId);

        var orchestratorSig = _signer.SignAcceptancePollVoteAgg(command.ThingId, offChainVotes, castedVoteEvents);

        var result = await _fileStorage.UploadJson(new
        {
            command.ThingId,
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
                    UserId = v.UserId, // @@TODO: EIP-55 encode
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
        foreach (var offChainVote in offChainVotes.OrderByDescending(v => v.CastedAtMs))
        {
            accountedVotes.Add(new()
            {
                VoterId = offChainVote.VoterId,
                VoteDecision = (AccountedVote.Decision)offChainVote.Decision
            });
        }

        var verifiers = await _contractCaller.GetVerifiersForThing(command.ThingId.ToByteArray());
        _logger.LogDebug("Thing {ThingId} Poll: {NumVerifiers} verifiers", command.ThingId, verifiers.Count());

        var notVotedVerifierIndices = verifiers
            .Select((verifier, i) => (VerifierId: verifier.Substring(2).ToLower(), Index: i))
            .Where(vi => accountedVotes.SingleOrDefault(v => v.VoterId == vi.VerifierId) == null)
            .Select(vi => (ulong)vi.Index)
            .ToList();

        _logger.LogDebug(
            "Thing {ThingId} Poll: {NumVerifiers} not voted. Slashing...",
            command.ThingId, notVotedVerifierIndices.Count
        );

        var numVerifiers = await _contractStorageQueryable.GetThingSubmissionNumVerifiers();
        int votingVolumeThresholdPercent = await _contractStorageQueryable.GetThingAcceptancePollVotingVolumeThreshold();

        var requiredVoterCount = Math.Ceiling(votingVolumeThresholdPercent / 100f * numVerifiers);
        _logger.LogDebug("Required voter count: {VoterCount}", requiredVoterCount);

        if (accountedVotes.Count < requiredVoterCount)
        {
            _logger.LogInformation(
                "Thing {ThingId} Lottery: Insufficient voting volume. " +
                "Required at least {RequiredVoterCount} voters out of {NumVerifiers} to vote; Got {ActualVoterCount}",
                command.ThingId, requiredVoterCount, numVerifiers, accountedVotes.Count
            );
            await _contractCaller.FinalizeAcceptancePollForThingAsUnsettledDueToInsufficientVotingVolume(
                command.ThingId.ToByteArray(), result.Data!, notVotedVerifierIndices
            );

            return VoidResult.Instance;
        }

        Debug.Assert(accountedVotes.Count > 0);

        int majorityThresholdPercent = await _contractStorageQueryable.GetThingAcceptancePollMajorityThreshold();
        Debug.Assert(majorityThresholdPercent >= 51);
        var acceptedDecisionRequiredVoteCount = Math.Ceiling(majorityThresholdPercent / 100f * accountedVotes.Count);
        _logger.LogDebug("Accepted decision required vote count: {VoteCount}", acceptedDecisionRequiredVoteCount);

        var votesGroupedByDecision = accountedVotes.GroupBy(v => v.VoteDecision);
        Debug.Assert(accountedVotes.Count == votesGroupedByDecision.Aggregate(0, (count, group) => count + group.Count()));
        var acceptedDecision = votesGroupedByDecision.MaxBy(group => group.Count())!;

        if (acceptedDecision.Count() < acceptedDecisionRequiredVoteCount)
        {
            _logger.LogInformation(
                "Thing {ThingId} Lottery: Majority threshold not reached. " +
                "Required at least {RequiredVoteCount} votes out of {TotalVoteCount}; Got {ActualVoteCount}",
                command.ThingId, acceptedDecisionRequiredVoteCount, accountedVotes.Count, acceptedDecision.Count()
            );
            await _contractCaller.FinalizeAcceptancePollForThingAsUnsettledDueToMajorityThresholdNotReached(
                command.ThingId.ToByteArray(), result.Data!, notVotedVerifierIndices
            );

            return VoidResult.Instance;
        }

        // @@NOTE: Since majorityThresholdPercent >= 51 and acceptedDecision.Count() >= acceptedDecisionRequiredVoteCount,
        // any other decisions could have only received fewer number of votes.

        // @@??: Should the not voted verifiers actually be slashed more than those who voted against the majority?
        // Should the soft declined verifiers be slashed less than Accepted ones when the accepted decision is hard decline?
        // And similarly for the other way around?
        var verifiersThatVotedAgainstTheMajority = votesGroupedByDecision
            .Where(v => v.Key != acceptedDecision.Key)
            .SelectMany(v => v)
            .Select(v => v.VoterId)
            .ToList();

        Debug.Assert(verifiersThatVotedAgainstTheMajority.Count() + acceptedDecision.Count() == accountedVotes.Count);

        _logger.LogInformation(
            "Thing {ThingId} Lottery: {Decision}. " +
            "Accepted decision: {NumVoters} voters. Other decisions: {NumOtherVoters} voters",
            command.ThingId, acceptedDecision.Key,
            acceptedDecision.Count(), verifiersThatVotedAgainstTheMajority.Count
        );

        var indicesOfVerifiersThatVotedAgainstTheMajority = verifiers
            .Select((verifier, i) => (VerifierId: verifier.Substring(2).ToLower(), Index: i))
            .Where(vi =>
                verifiersThatVotedAgainstTheMajority.SingleOrDefault(verifierId => verifierId == vi.VerifierId) != null
            )
            .Select(vi => (ulong)vi.Index);

        var verifiersToSlashIndices = notVotedVerifierIndices
            .Concat(indicesOfVerifiersThatVotedAgainstTheMajority)
            .Order()
            .ToList();

        Debug.Assert(verifiersToSlashIndices.Count + acceptedDecision.Count() == verifiers.Count());

        if (acceptedDecision.Key == AccountedVote.Decision.Accept)
        {
            await _contractCaller.FinalizeAcceptancePollForThingAsAccepted(
                thingId: command.ThingId.ToByteArray(),
                voteAggIpfsCid: result.Data!,
                verifiersToSlashIndices: verifiersToSlashIndices
            );

            return VoidResult.Instance;
        }

        throw new NotImplementedException();
    }
}
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
                Decision = (int)onChainVote.Decision
            });
        }
        foreach (var offChainVote in offChainVotes.OrderByDescending(v => v.CastedAtMs))
        {
            accountedVotes.Add(new()
            {
                VoterId = offChainVote.VoterId,
                Decision = (int)offChainVote.Decision
            });
        }

        // @@TODO!!: Calculate poll results!

        var numVerifiers = await _contractStorageQueryable.GetThingSubmissionNumVerifiers();
        var minimumVotingVolume = 50f / 100;

        if (accountedVotes.Count < Math.Ceiling(numVerifiers * minimumVotingVolume))
        {
            _logger.LogInformation("Thing {ThingId} Lottery: Insufficient voting volume", command.ThingId);

            var verifiers = await _contractCaller.GetVerifiersForThing(command.ThingId.ToByteArray());
            _logger.LogInformation("Thing {ThingId} Poll: {NumVerifiers} verifiers", command.ThingId, verifiers.Count());

            var verifiersToSlashIndices = verifiers
                .Select((verifier, i) => (VerifierId: verifier.Substring(2).ToLower(), Index: i))
                .Where(vi => accountedVotes.SingleOrDefault(v => v.VoterId == vi.VerifierId) == null)
                .Select(vi => (ulong)vi.Index)
                .ToList();

            _logger.LogInformation(
                "Thing {ThingId} Poll: {NumVerifiers} not voted. Slashing...",
                command.ThingId, verifiersToSlashIndices.Count
            );

            await _contractCaller.FinalizeAcceptancePollForThingAsUnsettledDueToInsufficientVotingVolume(
                command.ThingId.ToByteArray(), result.Data!, verifiersToSlashIndices
            );

            return VoidResult.Instance;
        }

        throw new NotImplementedException();

        /*
            - If not enough votes, put the thing in Stasis:
                - The submitter and all verifiers get their funds back.
                - But the verifiers that didn't vote get slashed.
                - No one is rewarded.
                - The submitter can revive the thing by moving it from Stasis to Awaiting Funding again.
                - Wipe the thing from the contracts like it never existed?
                - Allow the submitter to edit the thing when in Stasis before funding it again?
            - If majority threshold not reached, put the thing in Stasis:
                - The submitter and all verifiers get their funds back.
                - But the verifiers that didn't vote get slashed.
                - No one is rewarded.
                - The submitter can revive the thing by moving it from Stasis to Awaiting Funding again.
                - Wipe the thing from the contracts like it never existed?
                - Allow the submitter to edit the thing when in Stasis before funding it again?
        */

        // var votedVerifiers = accountedVotes
        //     .Select(v => v.VoterId)
        //     .ToList();

        // // @@TODO: Use queryable instead of repo.
        // var verifiers = (await _thingRepository.GetAllVerifiersFor(command.ThingId))
        //     .Select(v => v.VerifierId)
        //     .ToList();

        // var verifiersToSlash = verifiers.Except(votedVerifiers);
        // foreach (var verifier in verifiersToSlash)
        // {
        //     _logger.LogInformation("No vote from verifier {UserId}. Slashing...", verifier);
        // }

        // await _contractCaller.FinalizeAcceptancePollForThingAsAccepted(
        //     thingId: command.ThingId.ToByteArray(),
        //     voteAggIpfsCid: result.Data!,
        //     verifiersToReward: votedVerifiers.Select(v => "0x" + v).ToList(),
        //     verifiersToSlash: verifiersToSlash.Select(v => "0x" + v).ToList()
        // );

        return VoidResult.Instance;
    }
}
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
    private readonly IThingRepository _thingRepository;
    private readonly ISigner _signer;
    private readonly IFileStorage _fileStorage;
    private readonly IContractCaller _contractCaller;

    public CloseAcceptancePollCommandHandler(
        ILogger<CloseAcceptancePollCommandHandler> logger,
        IBlockchainQueryable blockchainQueryable,
        IAcceptancePollVoteRepository voteRepository,
        ICastedAcceptancePollVoteEventRepository castedAcceptancePollVoteEventRepository,
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

        var votedVerifiers = accountedVotes
            .Select(v => v.VoterId)
            .ToList();

        // @@TODO: Use queryable instead of repo.
        var verifiers = (await _thingRepository.GetAllVerifiersFor(command.ThingId))
            .Select(v => v.VerifierId)
            .ToList();

        var verifiersToSlash = verifiers.Except(votedVerifiers);
        foreach (var verifier in verifiersToSlash)
        {
            _logger.LogInformation("No vote from verifier {UserId}. Slashing...", verifier);
        }

        await _contractCaller.FinalizeAcceptancePollForThingAsAccepted(
            thingId: command.ThingId.ToByteArray(),
            voteAggIpfsCid: result.Data!,
            verifiersToReward: votedVerifiers.Select(v => "0x" + v).ToList(),
            verifiersToSlash: verifiersToSlash.Select(v => "0x" + v).ToList()
        );

        return VoidResult.Instance;
    }
}
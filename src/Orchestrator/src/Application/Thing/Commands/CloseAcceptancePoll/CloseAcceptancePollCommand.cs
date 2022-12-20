using Microsoft.Extensions.Logging;

using MediatR;

using Domain.Results;
using Domain.Aggregates;
using Domain.Aggregates.Events;

using Application.Common.Interfaces;
using Application.Vote.Commands.CastVote;

namespace Application.Thing.Commands.CloseAcceptancePoll;

internal class CloseAcceptancePollCommand : IRequest<VoidResult>
{
    public long LatestIncludedBlockNumber { get; init; }
    public Guid ThingId { get; init; }
}

internal class CloseAcceptancePollCommandHandler : IRequestHandler<CloseAcceptancePollCommand, VoidResult>
{
    private readonly ILogger<CloseAcceptancePollCommandHandler> _logger;
    private readonly IBlockchainQueryable _blockchainQueryable;
    private readonly IVoteRepository _voteRepository;
    private readonly ICastedAcceptancePollVoteEventRepository _castedAcceptancePollVoteEventRepository;
    private readonly IThingRepository _thingRepository;
    private readonly ISigner _signer;
    private readonly IFileStorage _fileStorage;
    private readonly IContractCaller _contractCaller;

    public CloseAcceptancePollCommandHandler(
        ILogger<CloseAcceptancePollCommandHandler> logger,
        IBlockchainQueryable blockchainQueryable,
        IVoteRepository voteRepository,
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

        var offChainVotes = await _voteRepository.GetForThingCastedAt(
            command.ThingId,
            noLaterThanTs: upperLimitTs,
            pollType: PollType.Acceptance
        );

        var castedVoteEvents = await _castedAcceptancePollVoteEventRepository.GetAllFor(command.ThingId);

        var orchestratorSig = _signer.SignVoteAgg(offChainVotes, castedVoteEvents);

        var result = await _fileStorage.UploadJson(new
        {
            OffChainVotes = offChainVotes
                .Select(v => new
                {
                    ThingId = v.ThingId.ToString(),
                    VoterId = "0x" + v.VoterId,
                    PollType = v.PollType.GetString(),
                    CastedAt = DateTimeOffset.FromUnixTimeMilliseconds(v.CastedAtMs).ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                    Decision = v.Decision.GetString(),
                    Reason = v.Reason ?? string.Empty,
                    IpfsCid = v.IpfsCid,
                    VoterSignature = v.VoterSignature
                }),
            OnChainVotes = castedVoteEvents
                .Select(v => new
                {
                    BlockNumber = v.BlockNumber,
                    TxnIndex = v.TxnIndex,
                    ThingIdHash = v.ThingIdHash,
                    UserId = "0x" + v.UserId,
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

        var votedVerifiers = offChainVotes
            .Select(v => v.VoterId)
            .Concat(castedVoteEvents.Select(e => e.UserId))
            .Distinct()
            .ToList();

        var verifiers = (await _thingRepository.GetAllVerifiersFor(command.ThingId))
            .Select(v => v.VerifierId)
            .ToList();
        var verifiersToSlash = verifiers.Except(votedVerifiers);

        await _contractCaller.FinalizeAcceptancePollForThingAsAccepted(
            thingId: command.ThingId.ToString(),
            voteAggIpfsCid: result.Data!,
            verifiersToReward: votedVerifiers.Select(v => "0x" + v).ToList(),
            verifiersToSlash: verifiersToSlash.Select(v => "0x" + v).ToList()
        );

        return VoidResult.Instance;
    }
}
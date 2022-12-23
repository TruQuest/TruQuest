using Microsoft.Extensions.Logging;

using MediatR;

using Domain.Aggregates;
using Domain.Aggregates.Events;
using Domain.Results;

using Application.Common.Interfaces;

namespace Application.Settlement.Commands.CloseAssessmentPoll;

internal class CloseAssessmentPollCommand : IRequest<VoidResult>
{
    public long LatestIncludedBlockNumber { get; init; }
    public Guid ThingId { get; init; }
    public Guid SettlementProposalId { get; init; }
}

internal class CloseAssessmentPollCommandHandler : IRequestHandler<CloseAssessmentPollCommand, VoidResult>
{
    private readonly ILogger<CloseAssessmentPollCommandHandler> _logger;
    private readonly IBlockchainQueryable _blockchainQueryable;
    private readonly IAssessmentPollVoteRepository _voteRepository;
    private readonly ICastedAssessmentPollVoteEventRepository _castedAssessmentPollVoteEventRepository;
    private readonly IThingRepository _thingRepository;
    private readonly ISettlementProposalRepository _settlementProposalRepository;
    private readonly ISigner _signer;
    private readonly IFileStorage _fileStorage;
    private readonly IContractCaller _contractCaller;

    public CloseAssessmentPollCommandHandler(
        ILogger<CloseAssessmentPollCommandHandler> logger,
        IBlockchainQueryable blockchainQueryable,
        IAssessmentPollVoteRepository voteRepository,
        ICastedAssessmentPollVoteEventRepository castedAssessmentPollVoteEventRepository,
        IThingRepository thingRepository,
        ISettlementProposalRepository settlementProposalRepository,
        ISigner signer,
        IFileStorage fileStorage,
        IContractCaller contractCaller
    )
    {
        _logger = logger;
        _blockchainQueryable = blockchainQueryable;
        _voteRepository = voteRepository;
        _castedAssessmentPollVoteEventRepository = castedAssessmentPollVoteEventRepository;
        _thingRepository = thingRepository;
        _settlementProposalRepository = settlementProposalRepository;
        _signer = signer;
        _fileStorage = fileStorage;
        _contractCaller = contractCaller;
    }

    public async Task<VoidResult> Handle(CloseAssessmentPollCommand command, CancellationToken ct)
    {
        long upperLimitTs = await _blockchainQueryable.GetBlockTimestamp(command.LatestIncludedBlockNumber);

        var offChainVotes = await _voteRepository.GetForThingSettlementProposalCastedAt(
            command.SettlementProposalId,
            noLaterThanTs: upperLimitTs
        );

        var castedVoteEvents = await _castedAssessmentPollVoteEventRepository.GetAllFor(
            command.ThingId, command.SettlementProposalId
        );

        var orchestratorSig = _signer.SignVoteAgg(offChainVotes, castedVoteEvents);

        var result = await _fileStorage.UploadJson(new
        {
            OffChainVotes = offChainVotes
                .Select(v => new
                {
                    SettlementProposalId = v.SettlementProposalId,
                    VoterId = "0x" + v.VoterId,
                    PollType = "Assessment",
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
                    SettlementProposalId = v.SettlementProposalId,
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

        var verifiers = (await _settlementProposalRepository.GetAllVerifiersFor(command.SettlementProposalId))
            .Select(v => v.VerifierId)
            .ToList();
        var verifiersToSlash = verifiers.Except(votedVerifiers);

        await _contractCaller.FinalizeAssessmentPollForSettlementProposalAsAccepted(
            thingId: command.ThingId.ToByteArray(),
            settlementProposalId: command.SettlementProposalId.ToByteArray(),
            voteAggIpfsCid: result.Data!,
            verifiersToReward: votedVerifiers.Select(v => "0x" + v).ToList(),
            verifiersToSlash: verifiersToSlash.Select(v => "0x" + v).ToList()
        );

        return VoidResult.Instance;
    }
}
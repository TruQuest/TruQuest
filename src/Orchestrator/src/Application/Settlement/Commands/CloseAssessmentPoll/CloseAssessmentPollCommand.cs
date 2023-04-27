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

        // @@TODO: Use queryable instead of repo.
        var offChainVotes = await _voteRepository.GetForThingSettlementProposalCastedAt(
            command.SettlementProposalId,
            noLaterThanTs: upperLimitTs
        );

        // @@TODO: Use queryable instead of repo.
        var castedVoteEvents = await _castedAssessmentPollVoteEventRepository.GetAllFor(
            command.ThingId, command.SettlementProposalId
        );

        var orchestratorSig = _signer.SignAssessmentPollVoteAgg(
            command.ThingId, command.SettlementProposalId, offChainVotes, castedVoteEvents
        );

        var result = await _fileStorage.UploadJson(new
        {
            command.ThingId,
            command.SettlementProposalId,
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
        var verifiers = (await _settlementProposalRepository.GetAllVerifiersFor(command.SettlementProposalId))
            .Select(v => v.VerifierId)
            .ToList();

        var verifiersToSlash = verifiers.Except(votedVerifiers);
        foreach (var verifier in verifiersToSlash)
        {
            _logger.LogInformation("No vote from verifier {UserId}. Slashing...", verifier);
        }

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
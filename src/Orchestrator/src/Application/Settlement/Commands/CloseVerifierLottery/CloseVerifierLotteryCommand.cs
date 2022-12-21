using Microsoft.Extensions.Logging;

using MediatR;

using Domain.Results;
using Domain.Aggregates;
using Domain.Aggregates.Events;

using Application.Common.Interfaces;

namespace Application.Settlement.Commands.CloseVerifierLottery;

internal class CloseVerifierLotteryCommand : IRequest<VoidResult>
{
    public long LatestIncludedBlockNumber { get; init; }
    public Guid ThingId { get; init; }
    public Guid SettlementProposalId { get; init; }
    public required byte[] Data { get; init; }
}

internal class CloseVerifierLotteryCommandHandler : IRequestHandler<CloseVerifierLotteryCommand, VoidResult>
{
    private readonly ILogger<CloseVerifierLotteryCommandHandler> _logger;
    private readonly IContractCaller _contractCaller;
    private readonly IThingRepository _thingRepository;
    private readonly IJoinedThingSubmissionVerifierLotteryEventRepository _joinedThingSubmissionVerifierLotteryEventRepository;
    private readonly IThingAssessmentVerifierLotterySpotClaimedEventRepository _thingAssessmentVerifierLotterySpotClaimedEventRepository;
    private readonly IPreJoinedThingAssessmentVerifierLotteryEventRepository _preJoinedThingAssessmentVerifierLotteryEventRepository;
    private readonly IJoinedThingAssessmentVerifierLotteryEventRepository _joinedThingAssessmentVerifierLotteryEventRepository;
    private readonly IContractStorageQueryable _contractStorageQueryable;

    public CloseVerifierLotteryCommandHandler(
        ILogger<CloseVerifierLotteryCommandHandler> logger,
        IContractCaller contractCaller,
        IThingRepository thingRepository,
        IJoinedThingSubmissionVerifierLotteryEventRepository joinedThingSubmissionVerifierLotteryEventRepository,
        IThingAssessmentVerifierLotterySpotClaimedEventRepository thingAssessmentVerifierLotterySpotClaimedEventRepository,
        IPreJoinedThingAssessmentVerifierLotteryEventRepository preJoinedThingAssessmentVerifierLotteryEventRepository,
        IJoinedThingAssessmentVerifierLotteryEventRepository joinedThingAssessmentVerifierLotteryEventRepository,
        IContractStorageQueryable contractStorageQueryable
    )
    {
        _logger = logger;
        _contractCaller = contractCaller;
        _thingRepository = thingRepository;
        _joinedThingSubmissionVerifierLotteryEventRepository = joinedThingSubmissionVerifierLotteryEventRepository;
        _thingAssessmentVerifierLotterySpotClaimedEventRepository = thingAssessmentVerifierLotterySpotClaimedEventRepository;
        _preJoinedThingAssessmentVerifierLotteryEventRepository = preJoinedThingAssessmentVerifierLotteryEventRepository;
        _joinedThingAssessmentVerifierLotteryEventRepository = joinedThingAssessmentVerifierLotteryEventRepository;
        _contractStorageQueryable = contractStorageQueryable;
    }

    public async Task<VoidResult> Handle(CloseVerifierLotteryCommand command, CancellationToken ct)
    {
        var thingId = command.ThingId.ToByteArray();
        var nonce = (decimal)await _contractCaller.ComputeNonceForThingAssessmentVerifierLottery(
            thingId, command.Data
        );
        int numVerifiers = await _contractStorageQueryable.GetThingAssessmentNumVerifiers();

        var thingSubmissionVerifiers = await _thingRepository.GetAllVerifiersFor(command.ThingId);

        var spotClaimedEvents = await _thingAssessmentVerifierLotterySpotClaimedEventRepository.FindAllFor(
            command.ThingId, command.SettlementProposalId
        );

        var spotClaimedEventsIndexed = spotClaimedEvents.Select((e, i) => (Event: e, Index: i)).ToList();

        var validSpotClaimedEvents = spotClaimedEventsIndexed
            .Where(e => thingSubmissionVerifiers.FirstOrDefault(v => v.VerifierId == e.Event.UserId) != null)
            .ToList();

        foreach (var @event in validSpotClaimedEvents)
        {
            var user = await _contractStorageQueryable.GetThingAssessmentVerifierLotterySpotClaimantAt(
                thingId,
                @event.Index
            );
            if (user.ToLower() != @event.Event.UserId)
            {
                throw new Exception("Incorrect winner selection");
            }
        }

        var joinedThingSubmissionVerifierLotteryEvents =
            await _joinedThingSubmissionVerifierLotteryEventRepository.FindWithClosestNoncesAmongUsers(
                thingId: command.ThingId,
                userIds: validSpotClaimedEvents.Select(e => e.Event.UserId).ToList(),
                nonce: nonce,
                count: thingSubmissionVerifiers.Count / 2
            );

        var winnerClaimantIndices = joinedThingSubmissionVerifierLotteryEvents.Join(
            validSpotClaimedEvents,
            je => je.UserId,
            ce => ce.Event.UserId,
            (je, ce) => (ulong)ce.Index
        ).ToList();

        int availableSpots = numVerifiers - winnerClaimantIndices.Count;

        var winnerEvents = await _joinedThingAssessmentVerifierLotteryEventRepository.FindWithClosestNonces(
            thingId: command.ThingId,
            settlementProposalId: command.SettlementProposalId,
            latestBlockNumber: command.LatestIncludedBlockNumber,
            nonce: nonce,
            count: availableSpots
        );

        if (winnerEvents.Count == availableSpots)
        {
            var lotteryWinners = await _preJoinedThingAssessmentVerifierLotteryEventRepository.GetLotteryWinnerIndices(
                command.ThingId,
                command.SettlementProposalId,
                winnerEvents.Select(e => e.UserId)
            );

            foreach (var winner in lotteryWinners)
            {
                var user = await _contractStorageQueryable.GetThingAssessmentVerifierLotteryParticipantAt(
                    thingId,
                    (int)winner.Index
                );
                if (user.ToLower() != winner.UserId)
                {
                    throw new Exception("Incorrect winner selection");
                }
            }

            await _contractCaller.CloseThingAssessmentVerifierLotteryWithSuccess(
                thingId,
                command.Data,
                winnerClaimantIndices,
                lotteryWinners.Select(w => w.Index).ToList()
            );
        }

        return VoidResult.Instance;
    }
}
using Microsoft.Extensions.Logging;

using MediatR;

using Domain.Results;
using Domain.Aggregates;

using Application.Common.Interfaces;

namespace Application.Settlement.Commands.CloseVerifierLottery;

internal class CloseVerifierLotteryCommand : IRequest<VoidResult>
{
    public required long LatestIncludedBlockNumber { get; init; }
    public required Guid ThingId { get; init; }
    public required Guid SettlementProposalId { get; init; }
    public required byte[] Data { get; init; }
}

internal class CloseVerifierLotteryCommandHandler : IRequestHandler<CloseVerifierLotteryCommand, VoidResult>
{
    private readonly ILogger<CloseVerifierLotteryCommandHandler> _logger;
    private readonly IContractCaller _contractCaller;
    private readonly IThingRepository _thingRepository;
    private readonly IThingAcceptancePollEventQueryable _thingAcceptancePollEventQueryable;
    private readonly IThingAssessmentPollEventQueryable _thingAssessmentPollEventQueryable;
    private readonly IContractStorageQueryable _contractStorageQueryable;

    public CloseVerifierLotteryCommandHandler(
        ILogger<CloseVerifierLotteryCommandHandler> logger,
        IContractCaller contractCaller,
        IThingRepository thingRepository,
        IThingAcceptancePollEventQueryable thingAcceptancePollEventQueryable,
        IThingAssessmentPollEventQueryable thingAssessmentPollEventQueryable,
        IContractStorageQueryable contractStorageQueryable
    )
    {
        _logger = logger;
        _contractCaller = contractCaller;
        _thingRepository = thingRepository;
        _thingAcceptancePollEventQueryable = thingAcceptancePollEventQueryable;
        _thingAssessmentPollEventQueryable = thingAssessmentPollEventQueryable;
        _contractStorageQueryable = contractStorageQueryable;
    }

    public async Task<VoidResult> Handle(CloseVerifierLotteryCommand command, CancellationToken ct)
    {
        var thingId = command.ThingId.ToByteArray();
        var proposalId = command.SettlementProposalId.ToByteArray();

        var nonce = (decimal)await _contractCaller.ComputeNonceForThingAssessmentVerifierLottery(
            thingId, proposalId, "Orchestrator", command.Data
        );
        int numVerifiers = await _contractStorageQueryable.GetThingAssessmentNumVerifiers();

        // @@TODO: Use queryable instead of repo.
        var thingSubmissionVerifiers = await _thingRepository.GetAllVerifiersFor(command.ThingId);

        var spotClaimedEvents = await _thingAssessmentPollEventQueryable.FindAllSpotClaimedEventsFor(
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
                proposalId,
                @event.Index
            );
            if (user.ToLower() != @event.Event.UserId)
            {
                throw new Exception("Incorrect claimant selection");
            }
        }

        var joinedThingSubmissionVerifierLotteryEvents = await _thingAcceptancePollEventQueryable.GetJoinedEventsFor(
            thingId: command.ThingId,
            userIds: validSpotClaimedEvents.Select(e => e.Event.UserId).ToList()
        );

        var winnerClaimantIndices = joinedThingSubmissionVerifierLotteryEvents
            .Join(
                validSpotClaimedEvents,
                je => je.UserId,
                ce => ce.Event.UserId,
                (je, ce) => new
                {
                    ce.Index,
                    je.Nonce
                }
            )
            .OrderBy(e => Math.Abs(nonce - e.Nonce))
                .ThenBy(e => e.Index)
            .Select(e => (ulong)e.Index)
            .Take(thingSubmissionVerifiers.Count / 2)
            .Order()
            .ToList();

        _logger.LogInformation(
            "Proposal {ProposalId} verifier lottery: {NumClaimants} spots claimed",
            command.SettlementProposalId,
            winnerClaimantIndices.Count
        );

        int availableSpots = numVerifiers - winnerClaimantIndices.Count;

        var winnerEvents = await _thingAssessmentPollEventQueryable.FindJoinedEventsWithClosestNonces(
            thingId: command.ThingId,
            settlementProposalId: command.SettlementProposalId,
            latestBlockNumber: command.LatestIncludedBlockNumber,
            nonce: nonce,
            count: availableSpots
        );

        if (winnerEvents.Count == availableSpots)
        {
            var lotteryWinners = await _thingAssessmentPollEventQueryable
                .GetLotteryWinnerIndicesAccordingToPreJoinedEvents(
                    command.ThingId,
                    command.SettlementProposalId,
                    winnerEvents.Select(e => e.UserId)
                );

            foreach (var winner in lotteryWinners)
            {
                var user = await _contractStorageQueryable.GetThingAssessmentVerifierLotteryParticipantAt(
                    thingId,
                    proposalId,
                    (int)winner.Index
                );
                if (user.ToLower() != winner.UserId)
                {
                    throw new Exception("Incorrect winner selection");
                }
            }

            await _contractCaller.CloseThingAssessmentVerifierLotteryWithSuccess(
                thingId,
                proposalId,
                command.Data,
                winnerClaimantIndices,
                lotteryWinners.Select(w => w.Index).ToList()
            );
        }
        else
        {
            _logger.LogInformation(
                "Proposal {ProposalId} assessment verifier selection lottery: Not enough participants",
                command.SettlementProposalId
            );
        }

        return VoidResult.Instance;
    }
}
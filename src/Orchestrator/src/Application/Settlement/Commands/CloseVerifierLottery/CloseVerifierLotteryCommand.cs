using System.Numerics;
using System.Diagnostics;

using Microsoft.Extensions.Logging;

using MediatR;

using Domain.Results;
using Domain.Aggregates.Events;

using Application.Common.Interfaces;
using Application.Common.Misc;

namespace Application.Settlement.Commands.CloseVerifierLottery;

internal class CloseVerifierLotteryCommand : IRequest<VoidResult>
{
    public required Guid ThingId { get; init; }
    public required Guid SettlementProposalId { get; init; }
    public required byte[] Data { get; init; }
    public required byte[] UserXorData { get; init; }
    public required long EndBlock { get; init; }
}

internal class CloseVerifierLotteryCommandHandler : IRequestHandler<CloseVerifierLotteryCommand, VoidResult>
{
    private readonly ILogger<CloseVerifierLotteryCommandHandler> _logger;
    private readonly IContractCaller _contractCaller;
    private readonly IL1BlockchainQueryable _l1BlockchainQueryable;
    private readonly IThingSubmissionVerifierLotteryEventQueryable _thingVerifierLotteryEventQueryable;
    private readonly ISettlementProposalAssessmentVerifierLotteryEventQueryable _proposalVerifierLotteryEventQueryable;
    private readonly IJoinedThingAssessmentVerifierLotteryEventRepository _joinedLotteryEventRepository;

    public CloseVerifierLotteryCommandHandler(
        ILogger<CloseVerifierLotteryCommandHandler> logger,
        IContractCaller contractCaller,
        IL1BlockchainQueryable l1BlockchainQueryable,
        IThingSubmissionVerifierLotteryEventQueryable thingVerifierLotteryEventQueryable,
        ISettlementProposalAssessmentVerifierLotteryEventQueryable proposalVerifierLotteryEventQueryable,
        IJoinedThingAssessmentVerifierLotteryEventRepository joinedLotteryEventRepository
    )
    {
        _logger = logger;
        _contractCaller = contractCaller;
        _l1BlockchainQueryable = l1BlockchainQueryable;
        _thingVerifierLotteryEventQueryable = thingVerifierLotteryEventQueryable;
        _proposalVerifierLotteryEventQueryable = proposalVerifierLotteryEventQueryable;
        _joinedLotteryEventRepository = joinedLotteryEventRepository;
    }

    public async Task<VoidResult> Handle(CloseVerifierLotteryCommand command, CancellationToken ct)
    {
        var thingId = command.ThingId.ToByteArray();
        var proposalId = command.SettlementProposalId.ToByteArray();
        bool expired = await _contractCaller.CheckThingAssessmentVerifierLotteryExpired(thingId, proposalId);
        Debug.Assert(expired);

        var endBlockHash = await _l1BlockchainQueryable.GetBlockHash(command.EndBlock);
        BigInteger maxNonce = await _contractCaller.GetThingAssessmentVerifierLotteryMaxNonce();

        var nonce = (long)(
            (
                new BigInteger(command.Data, isUnsigned: true, isBigEndian: true) ^
                new BigInteger(endBlockHash, isUnsigned: true, isBigEndian: true)
            ) % maxNonce
        );

        int numVerifiers = await _contractCaller.GetThingAssessmentLotteryNumVerifiers();

        var spotClaimedEvents = await _proposalVerifierLotteryEventQueryable.GetAllSpotClaimedEventsFor(
            command.ThingId, command.SettlementProposalId
        );

        var spotClaimedEventsIndexed = spotClaimedEvents.Select((e, i) => (Event: e, Index: i)).ToList();

        var spotClaimants = await _contractCaller.GetThingAssessmentVerifierLotterySpotClaimants(thingId, proposalId);
        foreach (var @event in spotClaimedEventsIndexed)
        {
            var claimantAtIndex = spotClaimants.ElementAtOrDefault(new Index(@event.Index));
            if (claimantAtIndex?.Substring(2).ToLower() != @event.Event.UserId)
            {
                throw new Exception("Incorrect claimant selection");
            }
        }

        var joinedThingSubmissionVerifierLotteryEvents = await _thingVerifierLotteryEventQueryable.GetJoinedEventsFor(
            thingId: command.ThingId,
            userIds: spotClaimedEvents.Select(e => e.UserId).ToList()
        );

        var winnerClaimantIndices = joinedThingSubmissionVerifierLotteryEvents
            .Join(
                spotClaimedEventsIndexed,
                je => je.UserId,
                ce => ce.Event.UserId,
                (je, ce) => new
                {
                    ce.Index,
                    je.Nonce
                }
            )
            .OrderBy(e => Math.Abs(nonce - e.Nonce!.Value))
                .ThenBy(e => e.Index)
            .Select(e => (ulong)e.Index)
            .Take(numVerifiers / 2) // @@TODO: Config.
            .Order()
            .ToList();

        _logger.LogInformation(
            "Proposal {ProposalId} verifier lottery: {NumClaimants} spots claimed",
            command.SettlementProposalId,
            winnerClaimantIndices.Count
        );

        int availableSpots = numVerifiers - winnerClaimantIndices.Count;

        var joinedEvents = await _joinedLotteryEventRepository.FindAllFor(command.ThingId, command.SettlementProposalId);
        foreach (var @event in joinedEvents)
        {
            @event.SetNonce((long)(
                (
                    new BigInteger(@event.UserData!.HexToByteArray(), isUnsigned: true, isBigEndian: true) ^
                    new BigInteger(command.UserXorData, isUnsigned: true, isBigEndian: true)
                ) % maxNonce
            ));
        }

        var winnerEventsIndexed = joinedEvents
            .Select((e, i) => (Index: (ulong)i, Event: e))
            .OrderBy(e => Math.Abs(nonce - e.Event.Nonce!.Value))
                .ThenBy(e => e.Index)
            .Take(availableSpots)
            .OrderBy(e => e.Index)
            .ToList();

        await _joinedLotteryEventRepository.UpdateNoncesFor(joinedEvents);
        await _joinedLotteryEventRepository.SaveChanges();

        if (winnerEventsIndexed.Count == availableSpots)
        {
            var participants = await _contractCaller.GetThingAssessmentVerifierLotteryParticipants(thingId, proposalId);
            foreach (var @event in winnerEventsIndexed)
            {
                var participantAtIndex = participants.ElementAtOrDefault(new Index((int)@event.Index));
                if (participantAtIndex?.Substring(2).ToLower() != @event.Event.UserId)
                {
                    throw new Exception("Incorrect winner selection");
                }
            }

            await _contractCaller.CloseThingAssessmentVerifierLotteryWithSuccess(
                thingId,
                proposalId,
                data: command.Data,
                userXorData: command.UserXorData,
                hashOfL1EndBlock: endBlockHash,
                winnerClaimantIndices: winnerClaimantIndices,
                winnerIndices: winnerEventsIndexed.Select(e => e.Index).ToList()
            );
        }
        else
        {
            _logger.LogInformation(
                "Proposal {ProposalId} Verifier Selection Lottery: Not enough participants.\n" +
                "Required: {RequiredNumVerifiers}.\n" +
                "Joined: {JoinedNumVerifiers}.",
                command.SettlementProposalId, numVerifiers, winnerClaimantIndices.Count + winnerEventsIndexed.Count
            );

            await _contractCaller.CloseThingAssessmentVerifierLotteryInFailure(
                thingId, proposalId, winnerClaimantIndices.Count + winnerEventsIndexed.Count
            );
        }

        return VoidResult.Instance;
    }
}

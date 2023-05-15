using MediatR;

using Domain.Aggregates;

using Application.Common.Attributes;

namespace Application.Ethereum.Events.AssessmentPoll.PollFinalized;

[ExecuteInTxn]
public class PollFinalizedEvent : INotification
{
    public required long BlockNumber { get; init; }
    public required int TxnIndex { get; init; }
    public required byte[] ThingId { get; init; }
    public required byte[] SettlementProposalId { get; init; }
    public required int Decision { get; init; }
    public required string VoteAggIpfsCid { get; init; }
}

internal class PollFinalizedEventHandler : INotificationHandler<PollFinalizedEvent>
{
    private readonly IThingRepository _thingRepository;
    private readonly ISettlementProposalRepository _settlementProposalRepository;
    private readonly ISettlementProposalUpdateRepository _settlementProposalUpdateRepository;
    private readonly IThingUpdateRepository _thingUpdateRepository;

    public PollFinalizedEventHandler(
        IThingRepository thingRepository,
        ISettlementProposalRepository settlementProposalRepository,
        ISettlementProposalUpdateRepository settlementProposalUpdateRepository,
        IThingUpdateRepository thingUpdateRepository
    )
    {
        _thingRepository = thingRepository;
        _settlementProposalRepository = settlementProposalRepository;
        _settlementProposalUpdateRepository = settlementProposalUpdateRepository;
        _thingUpdateRepository = thingUpdateRepository;
    }

    public async Task Handle(PollFinalizedEvent @event, CancellationToken ct)
    {
        var thing = await _thingRepository.FindById(new Guid(@event.ThingId));
        var proposal = await _settlementProposalRepository.FindById(new Guid(@event.SettlementProposalId));
        if (proposal.State == SettlementProposalState.VerifiersSelectedAndPollInitiated)
        {
            var decision = (AssessmentDecision)@event.Decision;
            if (decision == AssessmentDecision.Accepted)
            {
                proposal.SetState(SettlementProposalState.Accepted);
                thing.AcceptSettlementProposal(proposal.Id);
                thing.SetState(ThingState.Settled);

                await _thingUpdateRepository.AddOrUpdate(new ThingUpdate(
                    thingId: thing.Id,
                    category: ThingUpdateCategory.General,
                    updateTimestamp: DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    title: "Thing settled!",
                    details: "A settlement proposal has been accepted"
                ));
                await _thingUpdateRepository.SaveChanges();
            }
            else if (decision == AssessmentDecision.SoftDeclined)
            {
                proposal.SetState(SettlementProposalState.SoftDeclined);
            }
            else if (decision == AssessmentDecision.HardDeclined)
            {
                proposal.SetState(SettlementProposalState.HardDeclined);
            }

            proposal.SetVoteAggIpfsCid(@event.VoteAggIpfsCid);

            await _settlementProposalUpdateRepository.AddOrUpdate(new SettlementProposalUpdate(
                settlementProposalId: proposal.Id,
                category: SettlementProposalUpdateCategory.General,
                updateTimestamp: DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                title: "Proposal assessment poll completed",
                details: $"Decision: {decision}"
            ));

            await _settlementProposalRepository.SaveChanges();
            await _thingRepository.SaveChanges();
            await _settlementProposalUpdateRepository.SaveChanges();
        }
    }
}
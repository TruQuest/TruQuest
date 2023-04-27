using MediatR;

using Domain.Aggregates;

using Application.Common.Interfaces;

namespace Application.Ethereum.Events.AssessmentPoll.PollFinalized;

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
    private readonly IClientNotifier _clientNotifier;

    public PollFinalizedEventHandler(
        IThingRepository thingRepository,
        ISettlementProposalRepository settlementProposalRepository,
        IClientNotifier clientNotifier
    )
    {
        _thingRepository = thingRepository;
        _settlementProposalRepository = settlementProposalRepository;
        _clientNotifier = clientNotifier;
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

            await _settlementProposalRepository.SaveChanges();
            await _thingRepository.SaveChanges();

            await _clientNotifier.NotifySettlementProposalStateChanged(proposal.Id, proposal.State);
            if (decision == AssessmentDecision.Accepted)
            {
                await _clientNotifier.NotifyThingStateChanged(thing.Id, thing.State);
            }
        }
    }
}
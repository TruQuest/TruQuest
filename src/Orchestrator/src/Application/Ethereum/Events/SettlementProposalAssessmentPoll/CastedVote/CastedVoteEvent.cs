using MediatR;

using Domain.Aggregates;
using Domain.Aggregates.Events;

using Application.Ethereum.Common.Models.IM;

namespace Application.Ethereum.Events.SettlementProposalAssessmentPoll.CastedVote;

public class CastedVoteEvent : BaseContractEvent, INotification
{
    public required byte[] ThingId { get; init; }
    public required byte[] SettlementProposalId { get; init; }
    public required string WalletAddress { get; init; }
    public required int Vote { get; init; }
    public string? Reason { get; init; }
    public required long L1BlockNumber { get; init; }
}

internal class CastedVoteEventHandler : INotificationHandler<CastedVoteEvent>
{
    private readonly ICastedSettlementProposalAssessmentPollVoteEventRepository _castedAssessmentPollVoteEventRepository;

    public CastedVoteEventHandler(
        ICastedSettlementProposalAssessmentPollVoteEventRepository castedAssessmentPollVoteEventRepository
    )
    {
        _castedAssessmentPollVoteEventRepository = castedAssessmentPollVoteEventRepository;
    }

    public async Task Handle(CastedVoteEvent @event, CancellationToken ct)
    {
        var castedVoteEvent = new CastedSettlementProposalAssessmentPollVoteEvent(
            blockNumber: @event.BlockNumber,
            txnIndex: @event.TxnIndex,
            txnHash: @event.TxnHash,
            logIndex: @event.LogIndex,
            thingId: new Guid(@event.ThingId),
            settlementProposalId: new Guid(@event.SettlementProposalId),
            walletAddress: @event.WalletAddress,
            decision: (SettlementProposalAssessmentPollVote.VoteDecision)@event.Vote,
            reason: @event.Reason,
            l1BlockNumber: @event.L1BlockNumber
        );
        _castedAssessmentPollVoteEventRepository.Create(castedVoteEvent);

        await _castedAssessmentPollVoteEventRepository.SaveChanges();
    }
}

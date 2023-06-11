using MediatR;

using Domain.Aggregates;
using Domain.Aggregates.Events;

namespace Application.Ethereum.Events.AssessmentPoll.CastedVote;

public class CastedVoteEvent : INotification
{
    public required long BlockNumber { get; init; }
    public required int TxnIndex { get; init; }
    public required byte[] ThingId { get; init; }
    public required byte[] SettlementProposalId { get; init; }
    public required string UserId { get; init; }
    public required int Vote { get; init; }
    public string? Reason { get; init; }
    public required long L1BlockNumber { get; init; }
}

internal class CastedVoteEventHandler : INotificationHandler<CastedVoteEvent>
{
    private readonly ICastedAssessmentPollVoteEventRepository _castedAssessmentPollVoteEventRepository;

    public CastedVoteEventHandler(
        ICastedAssessmentPollVoteEventRepository castedAssessmentPollVoteEventRepository
    )
    {
        _castedAssessmentPollVoteEventRepository = castedAssessmentPollVoteEventRepository;
    }

    public async Task Handle(CastedVoteEvent @event, CancellationToken ct)
    {
        var castedVoteEvent = new CastedAssessmentPollVoteEvent(
            blockNumber: @event.BlockNumber,
            txnIndex: @event.TxnIndex,
            thingId: new Guid(@event.ThingId),
            settlementProposalId: new Guid(@event.SettlementProposalId),
            userId: @event.UserId,
            decision: (AssessmentPollVote.VoteDecision)@event.Vote,
            reason: @event.Reason,
            l1BlockNumber: @event.L1BlockNumber
        );
        _castedAssessmentPollVoteEventRepository.Create(castedVoteEvent);

        await _castedAssessmentPollVoteEventRepository.SaveChanges();
    }
}
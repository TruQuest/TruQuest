using MediatR;

using Domain.Aggregates;
using Domain.Aggregates.Events;
using CastedAssessmentPollVoteEventDm = Domain.Aggregates.Events.CastedAssessmentPollVoteEvent;

namespace Application.Ethereum.Events.AssessmentPoll.CastedAssessmentPollVote;

public class CastedAssessmentPollVoteEvent : INotification
{
    public long BlockNumber { get; init; }
    public int TxnIndex { get; init; }
    public required byte[] ThingId { get; init; }
    public required byte[] SettlementProposalId { get; init; }
    public required string UserId { get; init; }
    public int Vote { get; init; }
    public string? Reason { get; init; }
}

internal class CastedAssessmentPollVoteEventHandler : INotificationHandler<CastedAssessmentPollVoteEvent>
{
    private readonly ICastedAssessmentPollVoteEventRepository _castedAssessmentPollVoteEventRepository;

    public CastedAssessmentPollVoteEventHandler(
        ICastedAssessmentPollVoteEventRepository castedAssessmentPollVoteEventRepository
    )
    {
        _castedAssessmentPollVoteEventRepository = castedAssessmentPollVoteEventRepository;
    }

    public async Task Handle(CastedAssessmentPollVoteEvent @event, CancellationToken ct)
    {
        var castedVoteEvent = new CastedAssessmentPollVoteEventDm(
            blockNumber: @event.BlockNumber,
            txnIndex: @event.TxnIndex,
            thingId: new Guid(@event.ThingId),
            settlementProposalId: new Guid(@event.SettlementProposalId),
            userId: @event.UserId,
            decision: (AssessmentPollVote.VoteDecision)@event.Vote,
            reason: @event.Reason
        );
        _castedAssessmentPollVoteEventRepository.Create(castedVoteEvent);

        await _castedAssessmentPollVoteEventRepository.SaveChanges();
    }
}
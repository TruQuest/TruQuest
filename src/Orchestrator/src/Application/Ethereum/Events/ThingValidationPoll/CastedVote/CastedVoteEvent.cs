using GoThataway;

using Domain.Aggregates;
using Domain.Aggregates.Events;

using Application.Ethereum.Common.Models.IM;

namespace Application.Ethereum.Events.ThingValidationPoll.CastedVote;

public class CastedVoteEvent : BaseContractEvent, IEvent
{
    public required byte[] ThingId { get; init; }
    public required string WalletAddress { get; init; }
    public required int Vote { get; init; }
    public string? Reason { get; init; }
    public required long L1BlockNumber { get; init; }
}

public class CastedVoteEventHandler : IEventHandler<CastedVoteEvent>
{
    private readonly ICastedThingValidationPollVoteEventRepository _castedValidationPollVoteEventRepository;

    public CastedVoteEventHandler(
        ICastedThingValidationPollVoteEventRepository castedValidationPollVoteEventRepository
    )
    {
        _castedValidationPollVoteEventRepository = castedValidationPollVoteEventRepository;
    }

    public async Task Handle(CastedVoteEvent @event, CancellationToken ct)
    {
        var castedVoteEvent = new CastedThingValidationPollVoteEvent(
            blockNumber: @event.BlockNumber,
            txnIndex: @event.TxnIndex,
            txnHash: @event.TxnHash,
            logIndex: @event.LogIndex,
            thingId: new Guid(@event.ThingId),
            walletAddress: @event.WalletAddress,
            decision: (ThingValidationPollVote.VoteDecision)@event.Vote,
            reason: @event.Reason,
            l1BlockNumber: @event.L1BlockNumber
        );
        _castedValidationPollVoteEventRepository.Create(castedVoteEvent);

        await _castedValidationPollVoteEventRepository.SaveChanges();
    }
}

using GoThataway;

using Domain.Aggregates;
using Domain.Aggregates.Events;

using Application.Ethereum.Common.Models.IM;
using Application.Common.Monitoring;

namespace Application.Ethereum.Events.SettlementProposalAssessmentPoll.CastedVote;

public class CastedVoteEvent : BaseContractEvent, IEvent
{
    public required byte[] ThingId { get; init; }
    public required byte[] SettlementProposalId { get; init; }
    public required string WalletAddress { get; init; }
    public required int Vote { get; init; }
    public string? Reason { get; init; }
    public required long L1BlockNumber { get; init; }

    public IEnumerable<(string Name, object? Value)> GetActivityTags()
    {
        return new (string Name, object? Value)[]
        {
            (ActivityTags.SettlementProposalId, new Guid(SettlementProposalId)),
            (ActivityTags.WalletAddress, WalletAddress),
            (ActivityTags.Vote, ((SettlementProposalAssessmentPollVote.VoteDecision)Vote).ToString()),
            (ActivityTags.TxnHash, TxnHash)
        };
    }
}

public class CastedVoteEventHandler : IEventHandler<CastedVoteEvent>
{
    private readonly ICastedSettlementProposalAssessmentPollVoteEventRepository _voteEventRepository;

    public CastedVoteEventHandler(ICastedSettlementProposalAssessmentPollVoteEventRepository voteEventRepository)
    {
        _voteEventRepository = voteEventRepository;
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
        _voteEventRepository.Create(castedVoteEvent);

        await _voteEventRepository.SaveChanges();
    }
}

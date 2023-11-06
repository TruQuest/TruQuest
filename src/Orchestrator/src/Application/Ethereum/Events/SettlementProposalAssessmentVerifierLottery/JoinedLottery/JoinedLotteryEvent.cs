using MediatR;

using Domain.Aggregates.Events;

using Application.Common.Misc;
using Application.Ethereum.Common.Models.IM;

namespace Application.Ethereum.Events.SettlementProposalAssessmentVerifierLottery.JoinedLottery;

public class JoinedLotteryEvent : BaseContractEvent, INotification
{
    public required byte[] ThingId { get; init; }
    public required byte[] SettlementProposalId { get; init; }
    public required string WalletAddress { get; init; }
    public required byte[] UserData { get; init; }
    public required long L1BlockNumber { get; init; }
}

internal class JoinedLotteryEventHandler : INotificationHandler<JoinedLotteryEvent>
{
    private readonly IJoinedSettlementProposalAssessmentVerifierLotteryEventRepository _joinedAssessmentVerifierLotteryEventRepository;

    public JoinedLotteryEventHandler(
        IJoinedSettlementProposalAssessmentVerifierLotteryEventRepository joinedAssessmentVerifierLotteryEventRepository
    )
    {
        _joinedAssessmentVerifierLotteryEventRepository = joinedAssessmentVerifierLotteryEventRepository;
    }

    public async Task Handle(JoinedLotteryEvent @event, CancellationToken ct)
    {
        var joinedLotteryEvent = new JoinedSettlementProposalAssessmentVerifierLotteryEvent(
            blockNumber: @event.BlockNumber,
            txnIndex: @event.TxnIndex,
            txnHash: @event.TxnHash,
            logIndex: @event.LogIndex,
            thingId: new Guid(@event.ThingId),
            settlementProposalId: new Guid(@event.SettlementProposalId),
            walletAddress: @event.WalletAddress,
            l1BlockNumber: @event.L1BlockNumber,
            userData: @event.UserData.ToHex(prefix: true)
        );
        _joinedAssessmentVerifierLotteryEventRepository.Create(joinedLotteryEvent);

        await _joinedAssessmentVerifierLotteryEventRepository.SaveChanges();
    }
}

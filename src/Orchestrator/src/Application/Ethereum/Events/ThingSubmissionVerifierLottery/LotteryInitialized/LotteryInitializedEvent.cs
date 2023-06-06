using MediatR;

namespace Application.Ethereum.Events.ThingSubmissionVerifierLottery.LotteryInitialized;

public class LotteryInitializedEvent : INotification
{
    public required long BlockNumber { get; init; }
    public required int TxnIndex { get; init; }
    public required byte[] ThingId { get; init; }
    public required byte[] DataHash { get; init; }
    public required byte[] UserXorDataHash { get; init; }
}
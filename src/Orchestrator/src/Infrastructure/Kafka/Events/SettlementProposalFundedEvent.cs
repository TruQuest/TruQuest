namespace Infrastructure.Kafka.Events;

internal class SettlementProposalFundedEvent : TraceableEvent
{
    public required Guid SettlementProposalId { get; init; }
    public required string WalletAddress { get; init; }
    public required decimal Stake { get; init; }
}

namespace Infrastructure.Kafka.Events;

internal class ThingFundedEvent : TraceableEvent
{
    public required string WalletAddress { get; init; }
    public required decimal Stake { get; init; }
}

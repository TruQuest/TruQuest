namespace Infrastructure.Kafka.Events;

internal class ThingValidationPollFinalizedEvent : TraceableEvent
{
    public required int Decision { get; init; }
    public required string VoteAggIpfsCid { get; init; }
    public List<string> RewardedVerifiers { get; init; } = new();
    public List<string> SlashedVerifiers { get; init; } = new();
}

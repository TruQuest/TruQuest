namespace Infrastructure.Kafka.Events;

internal class ThingValidationVerifierLotteryClosedInFailureEvent : TraceableEvent
{
    public required int RequiredNumVerifiers { get; init; }
    public required int JoinedNumVerifiers { get; init; }
}

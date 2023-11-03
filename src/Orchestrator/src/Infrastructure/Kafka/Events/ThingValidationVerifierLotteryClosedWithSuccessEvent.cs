namespace Infrastructure.Kafka.Events;

internal class ThingValidationVerifierLotteryClosedWithSuccessEvent : TraceableEvent
{
    public required string Orchestrator { get; init; }
    public required string Data { get; init; }
    public required string UserXorData { get; init; }
    public required string HashOfL1EndBlock { get; init; }
    public required long Nonce { get; init; }
    public required List<string> WinnerWalletAddresses { get; init; }
}

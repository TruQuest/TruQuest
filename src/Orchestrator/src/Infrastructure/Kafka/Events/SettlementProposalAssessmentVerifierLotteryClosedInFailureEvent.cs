namespace Infrastructure.Kafka.Events;

internal class SettlementProposalAssessmentVerifierLotteryClosedInFailureEvent : TraceableEvent
{
    public required Guid SettlementProposalId { get; init; }
    public required int RequiredNumVerifiers { get; init; }
    public required int JoinedNumVerifiers { get; init; }
}

namespace Infrastructure.Kafka.Events;

internal class SettlementProposalAssessmentPollFinalizedEvent : TraceableEvent
{
    public required Guid SettlementProposalId { get; init; }
    public required int Decision { get; init; }
    public required string VoteAggIpfsCid { get; init; }
    public List<string> RewardedVerifiers { get; init; } = new();
    public List<string> SlashedVerifiers { get; init; } = new();
}

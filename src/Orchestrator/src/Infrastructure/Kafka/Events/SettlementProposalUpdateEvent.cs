using Domain.Aggregates;

namespace Infrastructure.Kafka.Events;

internal class SettlementProposalUpdateEvent : TraceableEvent
{
    public required Guid SettlementProposalId { get; init; }
    public required SettlementProposalUpdateCategory Category { get; init; }
    public required long UpdateTimestamp { get; init; }
    public required string Title { get; init; }
    public required string? Details { get; init; }
}

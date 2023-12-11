using KafkaFlow;

using Domain.Aggregates;
using Application.Common.Monitoring;

namespace Infrastructure.Kafka.Events;

internal class SettlementProposalUpdateEvent : TraceableEvent
{
    public required Guid SettlementProposalId { get; init; }
    public required SettlementProposalUpdateCategory Category { get; init; }
    public required long UpdateTimestamp { get; init; }
    public required string Title { get; init; }
    public required string? Details { get; init; }

    public override IEnumerable<(string Name, object? Value)> GetActivityTags(IMessageContext _)
    {
        return new (string Name, object? Value)[]
        {
            (ActivityTags.SettlementProposalId, SettlementProposalId),
            (ActivityTags.ItemUpdateCategory, Category.ToString())
        };
    }
}

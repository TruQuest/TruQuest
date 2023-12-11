using System.Text;

using KafkaFlow;

using Application.Common.Monitoring;

namespace Infrastructure.Kafka.Events;

internal class SettlementProposalFundedEvent : TraceableEvent
{
    public required Guid SettlementProposalId { get; init; }
    public required string WalletAddress { get; init; }
    public required decimal Stake { get; init; }

    public override IEnumerable<(string Name, object? Value)> GetActivityTags(IMessageContext context)
    {
        return new (string Name, object? Value)[]
        {
            (ActivityTags.ThingId, Guid.Parse(Encoding.UTF8.GetString((byte[])context.Message.Key))),
            (ActivityTags.SettlementProposalId, SettlementProposalId)
        };
    }
}

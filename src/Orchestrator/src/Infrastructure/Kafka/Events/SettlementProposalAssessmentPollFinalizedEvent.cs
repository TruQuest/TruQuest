using System.Text;

using KafkaFlow;

using Application.Common.Monitoring;

namespace Infrastructure.Kafka.Events;

internal class SettlementProposalAssessmentPollFinalizedEvent : TraceableEvent
{
    public required Guid SettlementProposalId { get; init; }
    public required int Decision { get; init; }
    public required string VoteAggIpfsCid { get; init; }
    public List<string> RewardedVerifiers { get; init; } = new();
    public List<string> SlashedVerifiers { get; init; } = new();

    public override IEnumerable<(string Name, object? Value)> GetActivityTags(IMessageContext context)
    {
        return new (string Name, object? Value)[]
        {
            (ActivityTags.ThingId, Guid.Parse(Encoding.UTF8.GetString((byte[])context.Message.Key))),
            (ActivityTags.SettlementProposalId, SettlementProposalId)
        };
    }
}

using System.Text;

using KafkaFlow;

using Application.Common.Monitoring;

namespace Infrastructure.Kafka.Events;

internal class ThingValidationVerifierLotteryClosedInFailureEvent : TraceableEvent
{
    public required int RequiredNumVerifiers { get; init; }
    public required int JoinedNumVerifiers { get; init; }

    public override IEnumerable<(string Name, object? Value)> GetActivityTags(IMessageContext context)
    {
        return new (string Name, object? Value)[]
        {
            (ActivityTags.ThingId, Guid.Parse(Encoding.UTF8.GetString((byte[])context.Message.Key)))
        };
    }
}

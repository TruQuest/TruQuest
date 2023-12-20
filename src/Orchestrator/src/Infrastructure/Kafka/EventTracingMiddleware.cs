using KafkaFlow;

using Application.Common.Monitoring;

using Infrastructure.Kafka.Events;

namespace Infrastructure.Kafka;

internal class EventTracingMiddleware : IMessageMiddleware
{
    public async Task Invoke(IMessageContext context, MiddlewareDelegate next)
    {
        var @event = (TraceableEvent)context.Message.Value;
        using var span = Telemetry.StartActivity(@event.GetType().GetActivityName(), traceparent: @event.Traceparent)!;
        await next(context);

        foreach (var tag in @event.GetActivityTags(context)) span.AddTag(tag.Name, tag.Value);
    }
}

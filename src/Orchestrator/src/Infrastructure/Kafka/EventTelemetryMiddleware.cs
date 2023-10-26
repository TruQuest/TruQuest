using KafkaFlow;

using Application;

using Infrastructure.Kafka.Messages;

namespace Infrastructure.Kafka;

internal class EventTelemetryMiddleware : IMessageMiddleware
{
    public async Task Invoke(IMessageContext context, MiddlewareDelegate next)
    {
        var @event = (TraceableEvent)context.Message.Value;
        using var span = Telemetry.StartActivity(@event.GetType().Name, traceparent: @event.Traceparent);
        await next(context);
    }
}

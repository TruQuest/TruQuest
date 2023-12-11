using System.Diagnostics;
using System.Text;

using KafkaFlow;

using Common.Monitoring;
using Messages.Requests;

internal class TelemetryMiddleware : IMessageMiddleware
{
    public async Task Invoke(IMessageContext context, MiddlewareDelegate next)
    {
        var parentSpanContext = Telemetry.ExtractContextFrom(
            context.Headers,
            (headers, key) =>
            {
                if (headers.Any(kv => kv.Key == key)) return Encoding.UTF8.GetString(headers[key]);
                return null;
            }
        );

        using var span = Telemetry.StartActivity(
            context.Message.Value.GetType().FullName!,
            parentContext: parentSpanContext,
            kind: ActivityKind.Consumer
        )!;

        await next(context);

        var message = (BaseRequest)context.Message.Value;
        foreach (var tag in message.GetActivityTags()) span.AddTag(tag.Name, tag.Value);
    }
}

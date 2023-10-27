using System.Diagnostics;
using System.Text;

using KafkaFlow;

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
            "Messages.Requests." + Encoding.UTF8.GetString(context.Headers["trq.requestType"]),
            parentContext: parentSpanContext,
            kind: ActivityKind.Consumer
        );

        await next(context);
    }
}

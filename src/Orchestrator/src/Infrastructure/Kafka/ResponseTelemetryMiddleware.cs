using System.Diagnostics;
using System.Text;

using KafkaFlow;

using Application;

namespace Infrastructure.Kafka;

internal class ResponseTelemetryMiddleware : IMessageMiddleware
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

        // @@??: Should add messaging tags here to connect both ends of communication ?

        using var span = Telemetry.StartActivity(
            Encoding.UTF8.GetString(context.Headers["trq.responseType"]),
            parentContext: parentSpanContext,
            kind: ActivityKind.Consumer
        );

        await next(context);
    }
}

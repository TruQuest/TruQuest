using System.Diagnostics;
using System.Text;

using KafkaFlow;

using Application;

namespace Infrastructure.Kafka;

internal class TelemetryMiddleware : IMessageMiddleware
{
    public async Task Invoke(IMessageContext context, MiddlewareDelegate next)
    {
        var parentSpanContext = Telemetry.ExtractContextFrom(
            context.Headers,
            (headers, key) =>
            {
                if (!key.StartsWith("trq.") && headers.Any(kv => kv.Key == key))
                {
                    return Encoding.UTF8.GetString(headers[key]);
                }
                return null;
            }
        );

        using var span = Telemetry.StartActivity(
            Encoding.UTF8.GetString(context.Headers["trq.responseType"]),
            ActivityKind.Consumer,
            parentContext: parentSpanContext
        );

        await next(context);
    }
}

using System.Diagnostics;
using System.Text;

using KafkaFlow;

using Application.Common.Monitoring;
using Application.Common.Messages.Responses;

namespace Infrastructure.Kafka;

internal class ResponseTracingMiddleware : IMessageMiddleware
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
            context.Message.Value.GetType().GetActivityName(),
            parentContext: parentSpanContext,
            kind: ActivityKind.Consumer
        )!;

        await next(context);

        var message = (BaseResponse)context.Message.Value;
        foreach (var tag in message.GetActivityTags()) span.AddTag(tag.Name, tag.Value);
    }
}

using System.Diagnostics;
using System.Collections.Concurrent;
using System.Text;

using KafkaFlow;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;

using Application;
using Application.Common.Interfaces;

namespace Infrastructure.Kafka;

internal class RequestDispatcher : IRequestDispatcher
{
    private readonly IMessageProducer<RequestDispatcher> _producer;

    private readonly ConcurrentDictionary<string, TaskCompletionSource<object>> _requestIdToResponseReceivedTcs = new();

    public RequestDispatcher(IMessageProducer<RequestDispatcher> producer)
    {
        _producer = producer;
    }

    public void SetResponseFor(string requestId, object response)
    {
        if (_requestIdToResponseReceivedTcs.TryRemove(requestId, out TaskCompletionSource<object>? tcs))
        {
            tcs.SetResult(response);
        }
    }

    public async Task<object> GetResult(object request)
    {
        TaskCompletionSource<object> tcs;
        using (var publishSpan = Telemetry.StartActivity("requests publish", ActivityKind.Client))
        {
            var context = publishSpan!.Context;
            var requestId = Guid.NewGuid().ToString();
            var messageKey = Guid.NewGuid().ToString();

            publishSpan.SetTag("messaging.system", "kafka");
            publishSpan.SetTag("messaging.operation", "publish");
            publishSpan.SetTag("messaging.message.conversation_id", requestId);
            publishSpan.SetTag("messaging.destination.name", "requests");
            publishSpan.SetTag("messaging.kafka.message.key", messageKey);

            var headers = new MessageHeaders
            {
                ["requestId"] = Encoding.UTF8.GetBytes(requestId)
            };

            Propagators.DefaultTextMapPropagator.Inject(
                new PropagationContext(context, Baggage.Current),
                headers,
                (headers, key, value) =>
                {
                    headers[key] = Encoding.UTF8.GetBytes(value);
                }
            );

            tcs = _requestIdToResponseReceivedTcs[requestId] = new();

            await _producer.ProduceAsync(
                messageKey: messageKey,
                messageValue: request,
                headers: headers
            );
        }

        using var waitForResponseSpan = Telemetry.StartActivity("responses wait");
        var result = await tcs.Task;

        return result;
    }

    public async Task Send(object request)
    {
        await _producer.ProduceAsync(
            messageKey: Guid.NewGuid().ToString(),
            messageValue: request
        );
    }
}
using System.Diagnostics;
using System.Collections.Concurrent;
using System.Text;
using System.Threading.Channels;

using Microsoft.Extensions.Hosting;

using KafkaFlow;

using Application;
using Application.Common.Interfaces;

namespace Infrastructure.Kafka;

internal class RequestDispatcher : IRequestDispatcher
{
    private readonly IMessageProducer<RequestDispatcher> _producer;

    private readonly ConcurrentDictionary<string, TaskCompletionSource<object>> _requestIdToResponseReceivedTcs = new();

    private readonly ChannelReader<(string RequestId, object Message)> _responseStream;
    public ChannelWriter<(string RequestId, object Message)> ResponseSink { get; }

    public RequestDispatcher(IMessageProducer<RequestDispatcher> producer, IHostApplicationLifetime appLifetime)
    {
        _producer = producer;

        var responseChannel = Channel.CreateUnbounded<(string RequestId, object Message)>(
            new UnboundedChannelOptions
            {
                SingleReader = true
            }
        );
        _responseStream = responseChannel.Reader;
        ResponseSink = responseChannel.Writer;

        var cts = CancellationTokenSource.CreateLinkedTokenSource(appLifetime.ApplicationStopping);
        _monitorResponses(cts.Token);
    }

    private async void _monitorResponses(CancellationToken ct)
    {
        try
        {
            await foreach (var response in _responseStream.ReadAllAsync(ct))
            {
                if (_requestIdToResponseReceivedTcs.TryRemove(response.RequestId, out TaskCompletionSource<object>? tcs))
                {
                    tcs.SetResult(response.Message);
                }
            }
        }
        catch (OperationCanceledException)
        {
            return;
        }
    }

    public async Task<object> GetResult(object request)
    {
        using var span = Telemetry.StartActivity(request.GetType().FullName!, kind: ActivityKind.Producer)!;

        var requestId = Guid.NewGuid().ToString();
        var messageKey = Guid.NewGuid().ToString();

        span.SetKafkaTags(requestId, messageKey, destinationName: "requests");

        var headers = new MessageHeaders
        {
            ["trq.requestId"] = Encoding.UTF8.GetBytes(requestId)
        };

        Telemetry.PropagateContextThrough(span.Context, headers, (headers, key, value) =>
        {
            headers[key] = Encoding.UTF8.GetBytes(value);
        });

        var tcs = _requestIdToResponseReceivedTcs[requestId] = new();

        await _producer.ProduceAsync(
            messageKey: messageKey,
            messageValue: request,
            headers: headers
        );

        var result = await tcs.Task;

        return result;
    }

    public async Task Send(object request)
    {
        using var span = Telemetry.StartActivity(request.GetType().FullName!, kind: ActivityKind.Producer)!;

        var requestId = Guid.NewGuid().ToString();
        var messageKey = Guid.NewGuid().ToString();

        span.SetKafkaTags(requestId, messageKey, destinationName: "requests");

        var headers = new MessageHeaders
        {
            ["trq.requestId"] = Encoding.UTF8.GetBytes(requestId)
        };

        Telemetry.PropagateContextThrough(span.Context, headers, (headers, key, value) =>
        {
            headers[key] = Encoding.UTF8.GetBytes(value);
        });

        await _producer.ProduceAsync(
            messageKey: messageKey,
            messageValue: request,
            headers: headers
        );
    }
}

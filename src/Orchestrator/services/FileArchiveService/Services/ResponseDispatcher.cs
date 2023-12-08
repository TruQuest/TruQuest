using System.Diagnostics;
using System.Text;

using KafkaFlow;

namespace Services;

internal class ResponseDispatcher : IResponseDispatcher
{
    private readonly string _topicName;
    private readonly byte[] _isResponseHeaderValue = { 1 };

    private readonly ILogger<ResponseDispatcher> _logger;
    private readonly IMessageProducer<ResponseDispatcher> _producer;

    public ResponseDispatcher(
        ILogger<ResponseDispatcher> logger,
        IConfiguration configuration,
        IMessageProducer<ResponseDispatcher> producer
    )
    {
        _logger = logger;
        _topicName = configuration["Kafka:Producer:Topic"]!;
        _producer = producer;
    }

    public async Task Reply(string requestId, object message)
    {
        using var span = Telemetry.StartActivity(message.GetType().FullName!, kind: ActivityKind.Producer)!;

        var messageKey = Guid.NewGuid().ToString();

        span.SetKafkaTags(requestId, messageKey, destinationName: _topicName);

        var headers = new MessageHeaders
        {
            ["trq.requestId"] = Encoding.UTF8.GetBytes(requestId),
            ["trq.isResponse"] = _isResponseHeaderValue
        };

        Telemetry.PropagateContextThrough(span.Context, headers, (headers, key, value) =>
        {
            headers[key] = Encoding.UTF8.GetBytes(value);
        });

        await _producer.ProduceAsync(
            messageKey: messageKey,
            messageValue: message,
            headers: headers
        );
    }

    public async Task Send(string requestId, object message, string? key = null)
    {
        using var span = Telemetry.StartActivity(message.GetType().FullName!, kind: ActivityKind.Producer)!;

        var messageKey = key ?? Guid.NewGuid().ToString();

        span.SetKafkaTags(requestId, messageKey, destinationName: _topicName);

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
            messageValue: message,
            headers: headers
        );
    }

    public void SendSync(string requestId, object message, string? key = null)
    {
        using var span = Telemetry.StartActivity(message.GetType().FullName!, kind: ActivityKind.Producer)!;

        var messageKey = key ?? Guid.NewGuid().ToString();

        span.SetKafkaTags(requestId, messageKey, destinationName: _topicName);

        var headers = new MessageHeaders
        {
            ["trq.requestId"] = Encoding.UTF8.GetBytes(requestId)
        };

        Telemetry.PropagateContextThrough(span.Context, headers, (headers, key, value) =>
        {
            headers[key] = Encoding.UTF8.GetBytes(value);
        });

        _producer.Produce(
            messageKey: messageKey,
            messageValue: message,
            headers: headers,
            deliveryHandler: report =>
            {
                if (report.Error.IsError)
                {
                    _logger.LogWarning(report.Error.Reason);
                }
            }
        );
    }
}

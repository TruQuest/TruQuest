using System.Text;
using System.Diagnostics;

using KafkaFlow;

namespace Services;

internal class ResponseDispatcher : IResponseDispatcher
{
    private readonly ILogger<ResponseDispatcher> _logger;
    private readonly IMessageProducer<ResponseDispatcher> _producer;

    public ResponseDispatcher(
        ILogger<ResponseDispatcher> logger,
        IMessageProducer<ResponseDispatcher> producer
    )
    {
        _logger = logger;
        _producer = producer;
    }

    public async Task ReplyTo(string requestId, object message, ActivityContext? parentContext = null)
    {
        using var span = Telemetry.StartActivity(
            "responses publish",
            ActivityKind.Server,
            parentContext: parentContext ?? new ActivityContext()
        );

        var messageKey = Guid.NewGuid().ToString();

        span!.SetTag("messaging.system", "kafka");
        span.SetTag("messaging.operation", "publish");
        span.SetTag("messaging.message.conversation_id", requestId);
        span.SetTag("messaging.destination.name", "responses");
        span.SetTag("messaging.kafka.message.key", messageKey);

        await _producer.ProduceAsync(
            messageKey: messageKey,
            messageValue: message,
            headers: new MessageHeaders
            {
                ["requestId"] = Encoding.UTF8.GetBytes(requestId)
            }
        );
    }

    public async Task SendAsync(object message, string? key = null)
    {
        await _producer.ProduceAsync(
            messageKey: key ?? Guid.NewGuid().ToString(),
            messageValue: message,
            headers: new MessageHeaders
            {
                ["requestId"] = Encoding.UTF8.GetBytes(Guid.Empty.ToString())
            }
        );
    }

    public void Send(object message, string? key = null)
    {
        _producer.Produce(
            messageKey: key ?? Guid.NewGuid().ToString(),
            messageValue: message,
            headers: new MessageHeaders
            {
                ["requestId"] = Encoding.UTF8.GetBytes(Guid.Empty.ToString())
            },
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
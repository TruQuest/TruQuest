using System.Text;

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

    public async Task ReplyTo(string requestId, object message)
    {
        await _producer.ProduceAsync(
            messageKey: Guid.NewGuid().ToString(),
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
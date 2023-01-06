using System.Text;

using KafkaFlow;

namespace Services;

internal class ResponseDispatcher : IResponseDispatcher
{
    private readonly IMessageProducer<ResponseDispatcher> _producer;

    public ResponseDispatcher(IMessageProducer<ResponseDispatcher> producer)
    {
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

    public async Task Send(object message, string? key = null)
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
}
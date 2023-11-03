using KafkaFlow;

using Application.Common.Interfaces;

namespace Infrastructure.Kafka;

internal class DeadLetterArchiver : IDeadLetterArchiver
{
    private readonly IMessageProducer<DeadLetterArchiver> _producer;

    public DeadLetterArchiver(IMessageProducer<DeadLetterArchiver> producer)
    {
        _producer = producer;
    }

    public Task Archive(object message, IEnumerable<KeyValuePair<string, byte[]>> headers) =>
        _producer.ProduceAsync(
            messageKey: Guid.NewGuid().ToString(),
            messageValue: message,
            headers: (IMessageHeaders)headers
        );
}

using System.Text;

using KafkaFlow;

using Domain.Aggregates.Events;

using Infrastructure.Kafka.Messages;

namespace Infrastructure.Kafka;

internal class MessageTypeResolver : IMessageTypeResolver
{
    private const string _messageTypeHeaderName = "Type";

    public Type OnConsume(IMessageContext context)
    {
        var messageTypeBytes = context.Headers[_messageTypeHeaderName];
        var eventType = (ThingEventType)int.Parse(Encoding.UTF8.GetString(messageTypeBytes));
        switch (eventType)
        {
            case ThingEventType.ThingFunded:
                return typeof(ThingFundedEvent);
            case ThingEventType.VerifierLotteryClosedWithSuccess:
                return typeof(VerifierLotteryClosedWithSuccessEvent);
        }

        throw new InvalidOperationException();
    }

    public void OnProduce(IMessageContext context)
    {
        throw new NotImplementedException();
    }
}
using System.Text;

using KafkaFlow;

using Domain.Aggregates.Events;

using Infrastructure.Kafka.Events;

namespace Infrastructure.Kafka;

internal class MessageTypeResolver : IMessageTypeResolver
{
    private const string _messageTypeHeaderName = "Type";

    public Type OnConsume(IMessageContext context)
    {
        if (context.ConsumerContext.Topic == "thing.events")
        {
            var messageTypeBytes = context.Headers[_messageTypeHeaderName];
            var eventType = ThingEventTypeExtension.FromString(Encoding.UTF8.GetString(messageTypeBytes));
            switch (eventType)
            {
                case ThingEventType.Funded:
                    return typeof(ThingFundedEvent);
                case ThingEventType.ValidationVerifierLotteryFailed:
                    return typeof(ThingValidationVerifierLotteryClosedInFailureEvent);
                case ThingEventType.ValidationVerifierLotterySucceeded:
                    return typeof(ThingValidationVerifierLotteryClosedWithSuccessEvent);
                case ThingEventType.ValidationPollFinalized:
                    return typeof(ThingValidationPollFinalizedEvent);
                case ThingEventType.SettlementProposalFunded:
                    return typeof(SettlementProposalFundedEvent);
                case ThingEventType.SettlementProposalAssessmentVerifierLotteryFailed:
                    throw new NotImplementedException(); // @@TODO!!
                case ThingEventType.SettlementProposalAssessmentVerifierLotterySucceeded:
                    return typeof(SettlementProposalAssessmentVerifierLotteryClosedWithSuccessEvent);
                case ThingEventType.SettlementProposalAssessmentPollFinalized:
                    return typeof(SettlementProposalAssessmentPollFinalizedEvent);
            }
        }
        else if (context.ConsumerContext.Topic == "updates")
        {
            var table = Encoding.UTF8.GetString(context.Headers["__table"]);
            if (table == "ThingUpdates")
            {
                return typeof(ThingUpdateEvent);
            }
            else if (table == "SettlementProposalUpdates")
            {
                return typeof(SettlementProposalUpdateEvent);
            }
        }

        throw new InvalidOperationException();
    }

    public void OnProduce(IMessageContext context)
    {
        context.Headers.SetString("trq.requestType", context.Message.Value.GetType().Name);
    }
}

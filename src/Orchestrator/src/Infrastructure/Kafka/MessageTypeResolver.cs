using System.Text;

using KafkaFlow;
using KafkaFlow.Middlewares.Serializer.Resolvers;

using Domain.Aggregates.Events;

using Infrastructure.Kafka.Events;

namespace Infrastructure.Kafka;

internal class MessageTypeResolver : IMessageTypeResolver
{
    private const string _messageTypeHeaderName = "Type";

    public ValueTask<Type> OnConsumeAsync(IMessageContext context)
    {
        Type? type = null;
        if (context.ConsumerContext.Topic == "thing.events")
        {
            var messageTypeBytes = context.Headers[_messageTypeHeaderName];
            var eventType = ThingEventTypeExtension.FromString(Encoding.UTF8.GetString(messageTypeBytes));
            switch (eventType)
            {
                case ThingEventType.Funded:
                    type = typeof(ThingFundedEvent);
                    break;
                case ThingEventType.ValidationVerifierLotteryFailed:
                    type = typeof(ThingValidationVerifierLotteryClosedInFailureEvent);
                    break;
                case ThingEventType.ValidationVerifierLotterySucceeded:
                    type = typeof(ThingValidationVerifierLotteryClosedWithSuccessEvent);
                    break;
                case ThingEventType.ValidationPollFinalized:
                    type = typeof(ThingValidationPollFinalizedEvent);
                    break;
                case ThingEventType.SettlementProposalFunded:
                    type = typeof(SettlementProposalFundedEvent);
                    break;
                case ThingEventType.SettlementProposalAssessmentVerifierLotteryFailed:
                    type = typeof(SettlementProposalAssessmentVerifierLotteryClosedInFailureEvent);
                    break;
                case ThingEventType.SettlementProposalAssessmentVerifierLotterySucceeded:
                    type = typeof(SettlementProposalAssessmentVerifierLotteryClosedWithSuccessEvent);
                    break;
                case ThingEventType.SettlementProposalAssessmentPollFinalized:
                    type = typeof(SettlementProposalAssessmentPollFinalizedEvent);
                    break;
            }
        }
        else if (context.ConsumerContext.Topic == "updates")
        {
            var table = Encoding.UTF8.GetString(context.Headers["__table"]);
            if (table == "ThingUpdates")
            {
                type = typeof(ThingUpdateEvent);
            }
            else if (table == "SettlementProposalUpdates")
            {
                type = typeof(SettlementProposalUpdateEvent);
            }
        }

        if (type != null)
        {
            return ValueTask.FromResult(type);
        }

        throw new InvalidOperationException();
    }

    public ValueTask OnProduceAsync(IMessageContext context)
    {
        context.Headers.SetString("trq.requestType", context.Message.Value.GetType().Name);
        return ValueTask.CompletedTask;
    }
}

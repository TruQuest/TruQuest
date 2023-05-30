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
        if (context.ConsumerContext.Topic == "thing.events")
        {
            var messageTypeBytes = context.Headers[_messageTypeHeaderName];
            var eventType = (ThingEventType)int.Parse(Encoding.UTF8.GetString(messageTypeBytes));
            switch (eventType)
            {
                case ThingEventType.Funded:
                    return typeof(ThingFundedEvent);
                case ThingEventType.SubmissionVerifierLotteryClosedInFailure:
                    return typeof(ThingSubmissionVerifierLotteryClosedInFailureEvent);
                case ThingEventType.SubmissionVerifierLotteryClosedWithSuccess:
                    return typeof(ThingSubmissionVerifierLotteryClosedWithSuccessEvent);
                case ThingEventType.SettlementProposalFunded:
                    return typeof(ThingSettlementProposalFundedEvent);
                case ThingEventType.SettlementProposalAssessmentVerifierLotteryClosedWithSuccess:
                    return typeof(ThingSettlementProposalAssessmentVerifierLotteryClosedWithSuccessEvent);
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
        context.Headers.SetString("requestType", context.Message.Value.GetType().Name);
    }
}
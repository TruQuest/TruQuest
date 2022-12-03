using KafkaFlow;
using KafkaFlow.TypedHandler;

namespace Infrastructure.Kafka.Messages;

internal class VerifierLotteryClosedWithSuccessEvent { }

internal class VerifierLotteryClosedWithSuccessEventHandler : IMessageHandler<VerifierLotteryClosedWithSuccessEvent>
{
    public async Task Handle(IMessageContext context, VerifierLotteryClosedWithSuccessEvent message)
    {
        throw new NotImplementedException();
    }
}
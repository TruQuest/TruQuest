using System.Text;

using KafkaFlow;
using KafkaFlow.TypedHandler;

using Application;
using Application.Thing.Commands.ArchiveDueToFailedLottery;

namespace Infrastructure.Kafka.Events;

internal class ThingValidationVerifierLotteryClosedInFailureEvent : TraceableEvent
{
    public required int RequiredNumVerifiers { get; init; }
    public required int JoinedNumVerifiers { get; init; }
}

internal class ThingValidationVerifierLotteryClosedInFailureEventHandler :
    IMessageHandler<ThingValidationVerifierLotteryClosedInFailureEvent>
{
    private readonly SenderWrapper _sender;

    public ThingValidationVerifierLotteryClosedInFailureEventHandler(SenderWrapper sender)
    {
        _sender = sender;
    }

    public Task Handle(IMessageContext context, ThingValidationVerifierLotteryClosedInFailureEvent message) =>
        _sender.Send(
            new ArchiveDueToFailedLotteryCommand
            {
                ThingId = Guid.Parse(Encoding.UTF8.GetString((byte[])context.Message.Key))
            },
            addToAdditionalSinks: true
        );
}

using System.Text;

using KafkaFlow;
using KafkaFlow.TypedHandler;

using Application;
using Application.Thing.Commands.ArchiveDueToFailedLottery;

namespace Infrastructure.Kafka.Messages;

internal class ThingSubmissionVerifierLotteryClosedInFailureEvent
{
    public required int RequiredNumVerifiers { get; init; }
    public required int JoinedNumVerifiers { get; init; }
}

internal class ThingSubmissionVerifierLotteryClosedInFailureEventHandler :
    IMessageHandler<ThingSubmissionVerifierLotteryClosedInFailureEvent>
{
    private readonly SenderWrapper _sender;

    public ThingSubmissionVerifierLotteryClosedInFailureEventHandler(SenderWrapper sender)
    {
        _sender = sender;
    }

    public Task Handle(IMessageContext context, ThingSubmissionVerifierLotteryClosedInFailureEvent message) =>
        _sender.Send(
            new ArchiveDueToFailedLotteryCommand
            {
                ThingId = Guid.Parse(Encoding.UTF8.GetString((byte[])context.Message.Key))
            },
            addToAdditionalSinks: true
        );
}

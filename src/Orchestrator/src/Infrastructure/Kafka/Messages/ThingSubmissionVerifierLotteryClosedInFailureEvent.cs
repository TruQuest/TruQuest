using System.Text;

using MediatR;
using KafkaFlow;
using KafkaFlow.TypedHandler;

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
    private readonly ISender _mediator;

    public ThingSubmissionVerifierLotteryClosedInFailureEventHandler(ISender mediator)
    {
        _mediator = mediator;
    }

    public Task Handle(IMessageContext context, ThingSubmissionVerifierLotteryClosedInFailureEvent message) =>
        _mediator.Send(new ArchiveDueToFailedLotteryCommand
        {
            ThingId = Guid.Parse(Encoding.UTF8.GetString((byte[])context.Message.Key))
        });
}
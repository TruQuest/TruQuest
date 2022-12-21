using System.Text;

using MediatR;
using KafkaFlow;
using KafkaFlow.TypedHandler;

using Application.Thing.Commands.PrepareForAcceptancePoll;

namespace Infrastructure.Kafka.Messages;

internal class ThingSubmissionVerifierLotteryClosedWithSuccessEvent
{
    public decimal Nonce { get; set; }
    public List<string> WinnerIds { get; set; }
}

internal class ThingSubmissionVerifierLotteryClosedWithSuccessEventHandler : IMessageHandler<ThingSubmissionVerifierLotteryClosedWithSuccessEvent>
{
    private const string _blockNumberHeaderName = "BlockNumber";

    private readonly ISender _mediator;

    public ThingSubmissionVerifierLotteryClosedWithSuccessEventHandler(ISender mediator)
    {
        _mediator = mediator;
    }

    public Task Handle(IMessageContext context, ThingSubmissionVerifierLotteryClosedWithSuccessEvent message) =>
        _mediator.Send(new PrepareForAcceptancePollCommand
        {
            AcceptancePollInitBlockNumber = long.Parse(Encoding.UTF8.GetString((byte[])context.Headers[_blockNumberHeaderName])),
            ThingId = Guid.Parse(Encoding.UTF8.GetString((byte[])context.Message.Key)),
            WinnerIds = message.WinnerIds
        });
}
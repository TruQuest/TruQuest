using System.Text;

using MediatR;
using KafkaFlow;
using KafkaFlow.TypedHandler;

using Application.Thing.Commands.PrepareForAcceptancePoll;

namespace Infrastructure.Kafka.Messages;

internal class VerifierLotteryClosedWithSuccessEvent
{
    public List<string> WinnerIds { get; set; }
}

internal class VerifierLotteryClosedWithSuccessEventHandler : IMessageHandler<VerifierLotteryClosedWithSuccessEvent>
{
    private const string _blockNumberHeaderName = "BlockNumber";

    private readonly ISender _mediator;

    public VerifierLotteryClosedWithSuccessEventHandler(ISender mediator)
    {
        _mediator = mediator;
    }

    public Task Handle(IMessageContext context, VerifierLotteryClosedWithSuccessEvent message) =>
        _mediator.Send(new PrepareForAcceptancePollCommand
        {
            AcceptancePollInitBlockNumber = long.Parse(Encoding.UTF8.GetString((byte[])context.Headers[_blockNumberHeaderName])),
            ThingIdHash = Encoding.UTF8.GetString((byte[])context.Message.Key),
            WinnerIds = message.WinnerIds
        });
}
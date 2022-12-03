using System.Text;

using MediatR;
using KafkaFlow;
using KafkaFlow.TypedHandler;

using Application.Thing.Commands.InitVerifierLottery;

namespace Infrastructure.Kafka.Messages;

internal class ThingFundedEvent
{
    public string UserId { get; set; }
    public decimal Stake { get; set; }
}

internal class ThingFundedEventHandler : IMessageHandler<ThingFundedEvent>
{
    private const string _blockNumberHeaderName = "BlockNumber";

    private readonly ISender _mediator;

    public ThingFundedEventHandler(ISender mediator)
    {
        _mediator = mediator;
    }

    public async Task Handle(IMessageContext context, ThingFundedEvent @event)
    {
        await _mediator.Send(new InitVerifierLotteryCommand
        {
            ThingFundedBlockNumber = long.Parse(Encoding.UTF8.GetString(context.Headers[_blockNumberHeaderName])),
            ThingIdHash = Encoding.UTF8.GetString((byte[])context.Message.Key)
        });
    }
}
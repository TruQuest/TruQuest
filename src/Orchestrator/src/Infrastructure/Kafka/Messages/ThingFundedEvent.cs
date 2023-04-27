using System.Text;

using MediatR;
using KafkaFlow;
using KafkaFlow.TypedHandler;

using Application.Thing.Commands.InitVerifierLottery;

namespace Infrastructure.Kafka.Messages;

internal class ThingFundedEvent
{
    public required string UserId { get; init; }
    public required decimal Stake { get; init; }
}

internal class ThingFundedEventHandler : IMessageHandler<ThingFundedEvent>
{
    private readonly ISender _mediator;

    public ThingFundedEventHandler(ISender mediator)
    {
        _mediator = mediator;
    }

    public Task Handle(IMessageContext context, ThingFundedEvent @event) =>
        _mediator.Send(new InitVerifierLotteryCommand
        {
            ThingId = Guid.Parse(Encoding.UTF8.GetString((byte[])context.Message.Key))
        });
}
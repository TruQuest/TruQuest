using System.Text;

using MediatR;
using KafkaFlow;
using KafkaFlow.TypedHandler;

using Application.Settlement.Commands.InitVerifierLottery;

namespace Infrastructure.Kafka.Messages;

internal class ThingSettlementProposalFundedEvent
{
    public Guid SettlementProposalId { get; set; }
    public string UserId { get; set; }
    public decimal Stake { get; set; }
}

internal class ThingSettlementProposalFundedEventHandler : IMessageHandler<ThingSettlementProposalFundedEvent>
{
    private readonly ISender _mediator;

    public ThingSettlementProposalFundedEventHandler(ISender mediator)
    {
        _mediator = mediator;
    }

    public Task Handle(IMessageContext context, ThingSettlementProposalFundedEvent @event) =>
        _mediator.Send(new InitVerifierLotteryCommand
        {
            ThingId = Guid.Parse(Encoding.UTF8.GetString((byte[])context.Message.Key)),
            SettlementProposalId = @event.SettlementProposalId
        });
}
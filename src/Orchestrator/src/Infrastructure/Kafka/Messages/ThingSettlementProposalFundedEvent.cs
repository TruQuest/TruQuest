using System.Text;

using KafkaFlow;
using KafkaFlow.TypedHandler;

using Application;
using Application.Settlement.Commands.InitVerifierLottery;

namespace Infrastructure.Kafka.Messages;

internal class ThingSettlementProposalFundedEvent : TraceableEvent
{
    public required Guid SettlementProposalId { get; init; }
    public required string UserId { get; init; }
    public required decimal Stake { get; init; }
}

internal class ThingSettlementProposalFundedEventHandler : IMessageHandler<ThingSettlementProposalFundedEvent>
{
    private readonly SenderWrapper _sender;

    public ThingSettlementProposalFundedEventHandler(SenderWrapper sender)
    {
        _sender = sender;
    }

    public Task Handle(IMessageContext context, ThingSettlementProposalFundedEvent @event) =>
        _sender.Send(
            new InitVerifierLotteryCommand
            {
                ThingId = Guid.Parse(Encoding.UTF8.GetString((byte[])context.Message.Key)),
                SettlementProposalId = @event.SettlementProposalId
            },
            addToAdditionalSinks: true
        );
}

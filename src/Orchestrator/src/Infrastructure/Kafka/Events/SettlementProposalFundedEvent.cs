using System.Text;

using KafkaFlow;
using KafkaFlow.TypedHandler;

using Application;
using Application.Settlement.Commands.InitVerifierLottery;

namespace Infrastructure.Kafka.Events;

internal class SettlementProposalFundedEvent : TraceableEvent
{
    public required Guid SettlementProposalId { get; init; }
    public required string WalletAddress { get; init; }
    public required decimal Stake { get; init; }
}

internal class SettlementProposalFundedEventHandler : IMessageHandler<SettlementProposalFundedEvent>
{
    private readonly SenderWrapper _sender;

    public SettlementProposalFundedEventHandler(SenderWrapper sender)
    {
        _sender = sender;
    }

    public Task Handle(IMessageContext context, SettlementProposalFundedEvent @event) =>
        _sender.Send(
            new InitVerifierLotteryCommand
            {
                ThingId = Guid.Parse(Encoding.UTF8.GetString((byte[])context.Message.Key)),
                SettlementProposalId = @event.SettlementProposalId
            },
            addToAdditionalSinks: true
        );
}

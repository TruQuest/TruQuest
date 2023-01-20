using System.Text;

using MediatR;
using KafkaFlow;
using KafkaFlow.TypedHandler;

using Application.Thing.Commands.PrepareForAcceptancePoll;

namespace Infrastructure.Kafka.Messages;

internal class ThingSubmissionVerifierLotteryClosedWithSuccessEvent
{
    public string Orchestrator { get; set; }
    public decimal Nonce { get; set; }
    public List<string> WinnerIds { get; set; }
}

internal class ThingSubmissionVerifierLotteryClosedWithSuccessEventHandler : IMessageHandler<ThingSubmissionVerifierLotteryClosedWithSuccessEvent>
{
    private const string _blockNumberHeaderName = "BlockNumber";
    private const string _txnIndexHeaderName = "TxnIndex";

    private readonly ISender _mediator;

    public ThingSubmissionVerifierLotteryClosedWithSuccessEventHandler(ISender mediator)
    {
        _mediator = mediator;
    }

    public Task Handle(IMessageContext context, ThingSubmissionVerifierLotteryClosedWithSuccessEvent message) =>
        _mediator.Send(new PrepareForAcceptancePollCommand
        {
            AcceptancePollInitBlockNumber = long.Parse(Encoding.UTF8.GetString((byte[])context.Headers[_blockNumberHeaderName])),
            AcceptancePollInitTxnIndex = int.Parse(Encoding.UTF8.GetString((byte[])context.Headers[_txnIndexHeaderName])),
            ThingId = Guid.Parse(Encoding.UTF8.GetString((byte[])context.Message.Key)),
            Orchestrator = message.Orchestrator,
            Nonce = message.Nonce,
            WinnerIds = message.WinnerIds
        });
}
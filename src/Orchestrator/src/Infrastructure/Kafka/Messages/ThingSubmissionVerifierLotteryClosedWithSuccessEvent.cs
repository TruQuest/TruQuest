using System.Text;

using MediatR;
using KafkaFlow;
using KafkaFlow.TypedHandler;

using Application.Thing.Commands.PrepareForAcceptancePoll;

namespace Infrastructure.Kafka.Messages;

internal class ThingSubmissionVerifierLotteryClosedWithSuccessEvent
{
    public required string Orchestrator { get; init; }
    public required string Data { get; init; }
    public required string UserXorData { get; init; }
    public required string HashOfL1EndBlock { get; init; }
    public required long Nonce { get; init; }
    public required List<string> WinnerIds { get; init; }
}

internal class ThingSubmissionVerifierLotteryClosedWithSuccessEventHandler :
    IMessageHandler<ThingSubmissionVerifierLotteryClosedWithSuccessEvent>
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
            AcceptancePollInitBlockNumber = long.Parse(
                Encoding.UTF8.GetString((byte[])context.Headers[_blockNumberHeaderName])
            ),
            AcceptancePollInitTxnIndex = int.Parse(
                Encoding.UTF8.GetString((byte[])context.Headers[_txnIndexHeaderName])
            ),
            ThingId = Guid.Parse(Encoding.UTF8.GetString((byte[])context.Message.Key)),
            Orchestrator = message.Orchestrator,
            Data = message.Data,
            UserXorData = message.UserXorData,
            HashOfL1EndBlock = message.HashOfL1EndBlock,
            Nonce = message.Nonce,
            WinnerIds = message.WinnerIds
        });
}
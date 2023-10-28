using System.Text;

using KafkaFlow;
using KafkaFlow.TypedHandler;

using Application;
using Application.Thing.Commands.PrepareForAcceptancePoll;

namespace Infrastructure.Kafka.Events;

internal class ThingSubmissionVerifierLotteryClosedWithSuccessEvent : TraceableEvent
{
    public required string Orchestrator { get; init; }
    public required string Data { get; init; }
    public required string UserXorData { get; init; }
    public required string HashOfL1EndBlock { get; init; }
    public required long Nonce { get; init; }
    public required List<string> WinnerWalletAddresses { get; init; }
}

internal class ThingSubmissionVerifierLotteryClosedWithSuccessEventHandler :
    IMessageHandler<ThingSubmissionVerifierLotteryClosedWithSuccessEvent>
{
    private const string _blockNumberHeaderName = "BlockNumber";
    private const string _txnIndexHeaderName = "TxnIndex";
    private const string _txnHashHeaderName = "TxnHash";

    private readonly SenderWrapper _sender;

    public ThingSubmissionVerifierLotteryClosedWithSuccessEventHandler(SenderWrapper sender)
    {
        _sender = sender;
    }

    public Task Handle(IMessageContext context, ThingSubmissionVerifierLotteryClosedWithSuccessEvent message) =>
        _sender.Send(
            new PrepareForAcceptancePollCommand
            {
                AcceptancePollInitBlockNumber = long.Parse(
                    Encoding.UTF8.GetString((byte[])context.Headers[_blockNumberHeaderName])
                ),
                AcceptancePollInitTxnIndex = int.Parse(
                    Encoding.UTF8.GetString((byte[])context.Headers[_txnIndexHeaderName])
                ),
                AcceptancePollInitTxnHash = Encoding.UTF8.GetString((byte[])context.Headers[_txnHashHeaderName]),
                ThingId = Guid.Parse(Encoding.UTF8.GetString((byte[])context.Message.Key)),
                Orchestrator = message.Orchestrator,
                Data = message.Data,
                UserXorData = message.UserXorData,
                HashOfL1EndBlock = message.HashOfL1EndBlock,
                Nonce = message.Nonce,
                WinnerWalletAddresses = message.WinnerWalletAddresses
            },
            addToAdditionalSinks: true
        );
}

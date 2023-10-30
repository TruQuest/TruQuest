using System.Text;

using KafkaFlow;
using KafkaFlow.TypedHandler;

using Application;
using Application.Thing.Commands.PrepareForValidationPoll;

namespace Infrastructure.Kafka.Events;

internal class ThingValidationVerifierLotteryClosedWithSuccessEvent : TraceableEvent
{
    public required string Orchestrator { get; init; }
    public required string Data { get; init; }
    public required string UserXorData { get; init; }
    public required string HashOfL1EndBlock { get; init; }
    public required long Nonce { get; init; }
    public required List<string> WinnerWalletAddresses { get; init; }
}

internal class ThingValidationVerifierLotteryClosedWithSuccessEventHandler :
    IMessageHandler<ThingValidationVerifierLotteryClosedWithSuccessEvent>
{
    private const string _blockNumberHeaderName = "BlockNumber";
    private const string _txnIndexHeaderName = "TxnIndex";
    private const string _txnHashHeaderName = "TxnHash";

    private readonly SenderWrapper _sender;

    public ThingValidationVerifierLotteryClosedWithSuccessEventHandler(SenderWrapper sender)
    {
        _sender = sender;
    }

    public Task Handle(IMessageContext context, ThingValidationVerifierLotteryClosedWithSuccessEvent message) =>
        _sender.Send(
            new PrepareForValidationPollCommand
            {
                PollInitBlockNumber = long.Parse(
                    Encoding.UTF8.GetString((byte[])context.Headers[_blockNumberHeaderName])
                ),
                PollInitTxnIndex = int.Parse(
                    Encoding.UTF8.GetString((byte[])context.Headers[_txnIndexHeaderName])
                ),
                PollInitTxnHash = Encoding.UTF8.GetString((byte[])context.Headers[_txnHashHeaderName]),
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

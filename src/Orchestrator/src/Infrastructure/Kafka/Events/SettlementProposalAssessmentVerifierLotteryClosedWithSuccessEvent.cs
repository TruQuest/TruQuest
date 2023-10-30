using System.Text;

using KafkaFlow;
using KafkaFlow.TypedHandler;

using Application;
using Application.Settlement.Commands.PrepareForAssessmentPoll;

namespace Infrastructure.Kafka.Events;

internal class SettlementProposalAssessmentVerifierLotteryClosedWithSuccessEvent : TraceableEvent
{
    public required Guid SettlementProposalId { get; init; }
    public required string Orchestrator { get; init; }
    public required string Data { get; init; }
    public required string UserXorData { get; init; }
    public required string HashOfL1EndBlock { get; init; }
    public required long Nonce { get; init; }
    // @@BUG: For some reason when claimants are empty debezium discards
    // the key-value pair entirely, so if this property is set to 'required'
    // json deserialization fails.
    public List<string> ClaimantWalletAddresses { get; init; } = new();
    public required List<string> WinnerWalletAddresses { get; init; }
}

internal class SettlementProposalAssessmentVerifierLotteryClosedWithSuccessEventHandler :
    IMessageHandler<SettlementProposalAssessmentVerifierLotteryClosedWithSuccessEvent>
{
    private const string _blockNumberHeaderName = "BlockNumber";
    private const string _txnIndexHeaderName = "TxnIndex";
    private const string _txnHashHeaderName = "TxnHash";

    private readonly SenderWrapper _sender;

    public SettlementProposalAssessmentVerifierLotteryClosedWithSuccessEventHandler(SenderWrapper sender)
    {
        _sender = sender;
    }

    public Task Handle(
        IMessageContext context, SettlementProposalAssessmentVerifierLotteryClosedWithSuccessEvent message
    ) => _sender.Send(
        new PrepareForAssessmentPollCommand
        {
            InitBlockNumber = long.Parse(
                Encoding.UTF8.GetString((byte[])context.Headers[_blockNumberHeaderName])
            ),
            InitTxnIndex = int.Parse(
                Encoding.UTF8.GetString((byte[])context.Headers[_txnIndexHeaderName])
            ),
            InitTxnHash = Encoding.UTF8.GetString((byte[])context.Headers[_txnHashHeaderName]),
            ThingId = Guid.Parse(Encoding.UTF8.GetString((byte[])context.Message.Key)),
            SettlementProposalId = message.SettlementProposalId,
            Orchestrator = message.Orchestrator,
            Data = message.Data,
            UserXorData = message.UserXorData,
            HashOfL1EndBlock = message.HashOfL1EndBlock,
            Nonce = message.Nonce,
            ClaimantWalletAddresses = message.ClaimantWalletAddresses,
            WinnerWalletAddresses = message.WinnerWalletAddresses
        },
        addToAdditionalSinks: true
    );
}

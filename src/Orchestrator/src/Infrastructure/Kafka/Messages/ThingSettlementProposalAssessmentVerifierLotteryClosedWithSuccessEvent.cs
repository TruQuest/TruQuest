using System.Text;

using MediatR;
using KafkaFlow;
using KafkaFlow.TypedHandler;

using Application.Settlement.Commands.PrepareForAssessmentPoll;

namespace Infrastructure.Kafka.Messages;

internal class ThingSettlementProposalAssessmentVerifierLotteryClosedWithSuccessEvent
{
    public Guid SettlementProposalId { get; set; }
    public string Orchestrator { get; set; }
    public decimal Nonce { get; set; }
    public List<string> ClaimantIds { get; set; }
    public List<string> WinnerIds { get; set; }
}

internal class ThingSettlementProposalAssessmentVerifierLotteryClosedWithSuccessEventHandler : IMessageHandler<ThingSettlementProposalAssessmentVerifierLotteryClosedWithSuccessEvent>
{
    private const string _blockNumberHeaderName = "BlockNumber";
    private const string _txnIndexHeaderName = "TxnIndex";

    private readonly ISender _mediator;

    public ThingSettlementProposalAssessmentVerifierLotteryClosedWithSuccessEventHandler(ISender mediator)
    {
        _mediator = mediator;
    }

    public Task Handle(IMessageContext context, ThingSettlementProposalAssessmentVerifierLotteryClosedWithSuccessEvent message) =>
        _mediator.Send(new PrepareForAssessmentPollCommand
        {
            AssessmentPollInitBlockNumber = long.Parse(Encoding.UTF8.GetString((byte[])context.Headers[_blockNumberHeaderName])),
            AssessmentPollInitTxnIndex = int.Parse(Encoding.UTF8.GetString((byte[])context.Headers[_txnIndexHeaderName])),
            ThingId = Guid.Parse(Encoding.UTF8.GetString((byte[])context.Message.Key)),
            SettlementProposalId = message.SettlementProposalId,
            Orchestrator = message.Orchestrator,
            Nonce = message.Nonce,
            ClaimantIds = message.ClaimantIds,
            WinnerIds = message.WinnerIds
        });
}
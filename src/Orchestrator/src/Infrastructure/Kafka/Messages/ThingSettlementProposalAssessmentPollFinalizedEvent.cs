using System.Text;

using KafkaFlow;
using KafkaFlow.TypedHandler;

using Domain.Aggregates;
using Application.Settlement.Commands.FinalizeAssessmentPoll;

namespace Infrastructure.Kafka.Messages;

internal class ThingSettlementProposalAssessmentPollFinalizedEvent
{
    public required Guid SettlementProposalId { get; init; }
    public required int Decision { get; init; }
    public required string VoteAggIpfsCid { get; init; }
    public List<string> RewardedVerifiers { get; init; } = new();
    public List<string> SlashedVerifiers { get; init; } = new();
}

internal class ThingSettlementProposalAssessmentPollFinalizedEventHandler :
    IMessageHandler<ThingSettlementProposalAssessmentPollFinalizedEvent>
{
    private readonly SenderWrapper _sender;

    public ThingSettlementProposalAssessmentPollFinalizedEventHandler(SenderWrapper sender)
    {
        _sender = sender;
    }

    public Task Handle(IMessageContext context, ThingSettlementProposalAssessmentPollFinalizedEvent message) =>
        _sender.Send(
            new FinalizeAssessmentPollCommand
            {
                ThingId = Guid.Parse(Encoding.UTF8.GetString((byte[])context.Message.Key)),
                SettlementProposalId = message.SettlementProposalId,
                Decision = (AssessmentDecision)message.Decision,
                VoteAggIpfsCid = message.VoteAggIpfsCid,
                RewardedVerifiers = message.RewardedVerifiers,
                SlashedVerifiers = message.SlashedVerifiers
            },
            addToAdditionalSinks: true
        );
}

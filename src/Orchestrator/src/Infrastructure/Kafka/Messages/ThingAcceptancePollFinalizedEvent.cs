using System.Text;

using KafkaFlow;
using KafkaFlow.TypedHandler;

using Domain.Aggregates;
using Application;
using Application.Thing.Commands.FinalizeAcceptancePoll;

namespace Infrastructure.Kafka.Messages;

internal class ThingAcceptancePollFinalizedEvent : TraceableEvent
{
    public required int Decision { get; init; }
    public required string VoteAggIpfsCid { get; init; }
    public List<string> RewardedVerifiers { get; init; } = new();
    public List<string> SlashedVerifiers { get; init; } = new();
}

internal class ThingAcceptancePollFinalizedEventHandler : IMessageHandler<ThingAcceptancePollFinalizedEvent>
{
    private readonly SenderWrapper _sender;

    public ThingAcceptancePollFinalizedEventHandler(SenderWrapper sender)
    {
        _sender = sender;
    }

    public Task Handle(IMessageContext context, ThingAcceptancePollFinalizedEvent message) =>
        _sender.Send(
            new FinalizeAcceptancePollCommand
            {
                ThingId = Guid.Parse(Encoding.UTF8.GetString((byte[])context.Message.Key)),
                Decision = (SubmissionEvaluationDecision)message.Decision,
                VoteAggIpfsCid = message.VoteAggIpfsCid,
                RewardedVerifiers = message.RewardedVerifiers,
                SlashedVerifiers = message.SlashedVerifiers
            },
            addToAdditionalSinks: true
        );
}

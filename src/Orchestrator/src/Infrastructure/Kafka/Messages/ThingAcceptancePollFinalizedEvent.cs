using System.Text;

using MediatR;
using KafkaFlow;
using KafkaFlow.TypedHandler;

using Domain.Aggregates;
using Application.Thing.Commands.FinalizeAcceptancePoll;

namespace Infrastructure.Kafka.Messages;

internal class ThingAcceptancePollFinalizedEvent
{
    public required int Decision { get; init; }
    public required string VoteAggIpfsCid { get; init; }
    public List<string> RewardedVerifiers { get; init; } = new();
    public List<string> SlashedVerifiers { get; init; } = new();
}

internal class ThingAcceptancePollFinalizedEventHandler : IMessageHandler<ThingAcceptancePollFinalizedEvent>
{
    private readonly ISender _mediator;

    public ThingAcceptancePollFinalizedEventHandler(ISender mediator)
    {
        _mediator = mediator;
    }

    public Task Handle(IMessageContext context, ThingAcceptancePollFinalizedEvent message) =>
        _mediator.Send(new FinalizeAcceptancePollCommand
        {
            ThingId = Guid.Parse(Encoding.UTF8.GetString((byte[])context.Message.Key)),
            Decision = (SubmissionEvaluationDecision)message.Decision,
            VoteAggIpfsCid = message.VoteAggIpfsCid,
            RewardedVerifiers = message.RewardedVerifiers,
            SlashedVerifiers = message.SlashedVerifiers
        });
}
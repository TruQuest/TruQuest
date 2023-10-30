using System.Text;

using KafkaFlow;
using KafkaFlow.TypedHandler;

using Domain.Aggregates;
using Application;
using Application.Thing.Commands.FinalizeValidationPoll;

namespace Infrastructure.Kafka.Events;

internal class ThingValidationPollFinalizedEvent : TraceableEvent
{
    public required int Decision { get; init; }
    public required string VoteAggIpfsCid { get; init; }
    public List<string> RewardedVerifiers { get; init; } = new();
    public List<string> SlashedVerifiers { get; init; } = new();
}

internal class ThingValidationPollFinalizedEventHandler : IMessageHandler<ThingValidationPollFinalizedEvent>
{
    private readonly SenderWrapper _sender;

    public ThingValidationPollFinalizedEventHandler(SenderWrapper sender)
    {
        _sender = sender;
    }

    public Task Handle(IMessageContext context, ThingValidationPollFinalizedEvent message) =>
        _sender.Send(
            new FinalizeValidationPollCommand
            {
                ThingId = Guid.Parse(Encoding.UTF8.GetString((byte[])context.Message.Key)),
                Decision = (ValidationDecision)message.Decision,
                VoteAggIpfsCid = message.VoteAggIpfsCid,
                RewardedVerifiers = message.RewardedVerifiers,
                SlashedVerifiers = message.SlashedVerifiers
            },
            addToAdditionalSinks: true
        );
}

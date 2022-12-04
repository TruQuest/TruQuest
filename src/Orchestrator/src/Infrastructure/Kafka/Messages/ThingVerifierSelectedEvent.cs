using MediatR;
using KafkaFlow;
using KafkaFlow.TypedHandler;

using Application.User.Commands.NotifyAboutSelectionAsVerifier;

namespace Infrastructure.Kafka.Messages;

internal class ThingVerifierSelectedEvent
{
    public Guid ThingId { get; set; }
    public string VerifierId { get; set; }
}

internal class ThingVerifierSelectedEventHandler : IMessageHandler<ThingVerifierSelectedEvent>
{
    private readonly ISender _mediator;

    public ThingVerifierSelectedEventHandler(ISender mediator)
    {
        _mediator = mediator;
    }

    public Task Handle(IMessageContext context, ThingVerifierSelectedEvent message) =>
        _mediator.Send(new NotifyAboutSelectionAsVerifierCommand
        {
            ThingId = message.ThingId,
            UserId = message.VerifierId
        });
}
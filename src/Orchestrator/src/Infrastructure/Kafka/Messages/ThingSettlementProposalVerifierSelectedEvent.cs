using MediatR;
using KafkaFlow;
using KafkaFlow.TypedHandler;

using Application.User.Commands.NotifyAboutSelectionAsVerifier;

namespace Infrastructure.Kafka.Messages;

internal class ThingSettlementProposalVerifierSelectedEvent
{
    public Guid SettlementProposalId { get; set; }
    public string VerifierId { get; set; }
}

internal class ThingSettlementProposalVerifierSelectedEventHandler : IMessageHandler<ThingSettlementProposalVerifierSelectedEvent>
{
    private readonly ISender _mediator;

    public ThingSettlementProposalVerifierSelectedEventHandler(ISender mediator)
    {
        _mediator = mediator;
    }

    public Task Handle(IMessageContext context, ThingSettlementProposalVerifierSelectedEvent message) =>
        _mediator.Send(new NotifyAboutSelectionAsVerifierCommand
        {
            SettlementProposalId = message.SettlementProposalId,
            UserId = message.VerifierId
        });
}
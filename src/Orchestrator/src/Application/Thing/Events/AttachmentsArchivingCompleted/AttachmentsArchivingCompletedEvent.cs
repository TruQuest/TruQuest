using MediatR;

using Application.Thing.Common.Models.IM;
using Application.Common.Interfaces;

namespace Application.Thing.Events.AttachmentsArchivingCompleted;

public class AttachmentsArchivingCompletedEvent : INotification
{
    public required string SubmitterId { get; init; }
    public Guid ThingId { get; init; }
    public required NewThingIm Input { get; init; }
}

internal class AttachmentsArchivingCompletedEventHandler : INotificationHandler<AttachmentsArchivingCompletedEvent>
{
    private readonly IClientNotifier _clientNotifier;

    public AttachmentsArchivingCompletedEventHandler(IClientNotifier clientNotifier)
    {
        _clientNotifier = clientNotifier;
    }

    public async Task Handle(AttachmentsArchivingCompletedEvent @event, CancellationToken ct)
    {
        await _clientNotifier.TellAboutNewThingDraftCreationProgress(@event.SubmitterId, @event.ThingId, 100);
    }
}
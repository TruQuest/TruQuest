using MediatR;

using Application.Common.Interfaces;

namespace Application.Thing.Events.AttachmentsArchivingProgress;

public class AttachmentsArchivingProgressEvent : INotification
{
    public required string SubmitterId { get; init; }
    public Guid ThingId { get; init; }
    public int Percent { get; init; }
}

internal class AttachmentsArchivingProgressEventHandler : INotificationHandler<AttachmentsArchivingProgressEvent>
{
    private readonly IClientNotifier _clientNotifier;

    public AttachmentsArchivingProgressEventHandler(IClientNotifier clientNotifier)
    {
        _clientNotifier = clientNotifier;
    }

    public Task Handle(AttachmentsArchivingProgressEvent @event, CancellationToken ct) =>
        _clientNotifier.TellAboutNewThingDraftCreationProgress(
            @event.SubmitterId, @event.ThingId, @event.Percent
        );
}
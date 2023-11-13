using GoThataway;

using Application.Common.Interfaces;

namespace Application.Thing.Events.AttachmentsArchivingProgress;

public class AttachmentsArchivingProgressEvent : IEvent
{
    public required string SubmitterId { get; init; }
    public required Guid ThingId { get; init; }
    public required int Percent { get; init; }
}

public class AttachmentsArchivingProgressEventHandler : IEventHandler<AttachmentsArchivingProgressEvent>
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

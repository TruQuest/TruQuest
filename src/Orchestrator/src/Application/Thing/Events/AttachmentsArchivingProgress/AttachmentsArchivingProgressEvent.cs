using GoThataway;

using Application.Common.Interfaces;
using Application.Common.Monitoring;

namespace Application.Thing.Events.AttachmentsArchivingProgress;

public class AttachmentsArchivingProgressEvent : IEvent
{
    public required string SubmitterId { get; init; }
    public required Guid ThingId { get; init; }
    public required int Percent { get; init; }

    public IEnumerable<(string Name, object? Value)> GetActivityTags()
    {
        return new (string Name, object? Value)[]
        {
            (ActivityTags.UserId, SubmitterId),
            (ActivityTags.ThingId, ThingId)
        };
    }
}

public class AttachmentsArchivingProgressEventHandler : IEventHandler<AttachmentsArchivingProgressEvent>
{
    private readonly IClientNotifier _clientNotifier;

    public AttachmentsArchivingProgressEventHandler(IClientNotifier clientNotifier)
    {
        _clientNotifier = clientNotifier;
    }

    public Task Handle(AttachmentsArchivingProgressEvent @event, CancellationToken ct) =>
        _clientNotifier.TellAboutNewThingDraftCreationProgress(@event.SubmitterId, @event.ThingId, @event.Percent);
}

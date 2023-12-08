using Microsoft.Extensions.Logging;

using GoThataway;

using Application.Common.Interfaces;

namespace Application.Thing.Events.AttachmentsArchivingFailed;

public class AttachmentsArchivingFailedEvent : IEvent
{
    public required string SubmitterId { get; init; }
    public required Guid ThingId { get; init; }
    public required string ErrorMessage { get; init; }
}

public class AttachmentsArchivingFailedEventHandler : IEventHandler<AttachmentsArchivingFailedEvent>
{
    private readonly ILogger<AttachmentsArchivingFailedEventHandler> _logger;
    private readonly IClientNotifier _clientNotifier;

    public AttachmentsArchivingFailedEventHandler(
        ILogger<AttachmentsArchivingFailedEventHandler> logger,
        IClientNotifier clientNotifier
    )
    {
        _logger = logger;
        _clientNotifier = clientNotifier;
    }

    public async Task Handle(AttachmentsArchivingFailedEvent @event, CancellationToken ct)
    {
        _logger.LogWarning(
            "Error trying to archive attachments for thing {ThingId}: " + @event.ErrorMessage,
            @event.ThingId
        );

        await _clientNotifier.TellAboutNewThingDraftCreationProgress(@event.SubmitterId, @event.ThingId, -100);
    }
}

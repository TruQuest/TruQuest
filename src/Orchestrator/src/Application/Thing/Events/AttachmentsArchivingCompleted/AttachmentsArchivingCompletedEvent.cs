using Microsoft.Extensions.Logging;

using GoThataway;

using Domain.Aggregates;
using ThingDm = Domain.Aggregates.Thing;

using Application.Thing.Common.Models.IM;
using Application.Common.Interfaces;
using Application.Common.Attributes;
using Application.Common.Monitoring;
using static Application.Common.Monitoring.LogMessagePlaceholders;

namespace Application.Thing.Events.AttachmentsArchivingCompleted;

[ExecuteInTxn]
public class AttachmentsArchivingCompletedEvent : IEvent
{
    public required string SubmitterId { get; init; }
    public required Guid ThingId { get; init; }
    public required NewThingIm Input { get; init; }

    public IEnumerable<(string Name, object? Value)> GetActivityTags()
    {
        return new (string Name, object? Value)[]
        {
            (ActivityTags.UserId, SubmitterId),
            (ActivityTags.ThingId, ThingId)
        };
    }
}

public class AttachmentsArchivingCompletedEventHandler : IEventHandler<AttachmentsArchivingCompletedEvent>
{
    private readonly ILogger<AttachmentsArchivingCompletedEventHandler> _logger;
    private readonly IClientNotifier _clientNotifier;
    private readonly IThingRepository _thingRepository;
    private readonly IThingUpdateRepository _thingUpdateRepository;
    private readonly IWatchedItemRepository _watchedItemRepository;

    public AttachmentsArchivingCompletedEventHandler(
        ILogger<AttachmentsArchivingCompletedEventHandler> logger,
        IClientNotifier clientNotifier,
        IThingRepository thingRepository,
        IThingUpdateRepository thingUpdateRepository,
        IWatchedItemRepository watchedItemRepository
    )
    {
        _logger = logger;
        _clientNotifier = clientNotifier;
        _thingRepository = thingRepository;
        _thingUpdateRepository = thingUpdateRepository;
        _watchedItemRepository = watchedItemRepository;
    }

    public async Task Handle(AttachmentsArchivingCompletedEvent @event, CancellationToken ct)
    {
        await _clientNotifier.TellAboutNewThingDraftCreationProgress(@event.SubmitterId, @event.ThingId, percent: 100);

        var thing = new ThingDm(
            id: @event.ThingId,
            title: @event.Input.Title,
            details: @event.Input.Details,
            imageIpfsCid: @event.Input.ImageIpfsCid,
            croppedImageIpfsCid: @event.Input.CroppedImageIpfsCid,
            submitterId: @event.SubmitterId,
            subjectId: @event.Input.SubjectId
        );
        thing.AddEvidence(@event.Input.Evidence.Select(e =>
        {
            return new ThingEvidence(
                originUrl: e.Url,
                ipfsCid: e.IpfsCid!,
                previewImageIpfsCid: e.PreviewImageIpfsCid!
            );
        }));
        thing.AddTags(@event.Input.Tags.Select(t => t.Id));

        _thingRepository.Create(thing);

        var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        _watchedItemRepository.Add(new WatchedItem(
            userId: @event.SubmitterId,
            itemType: WatchedItemType.Thing,
            itemId: thing.Id,
            itemUpdateCategory: (int)ThingUpdateCategory.General,
            lastSeenUpdateTimestamp: now
        ));

        await _thingUpdateRepository.AddOrUpdate(new ThingUpdate(
            thingId: thing.Id,
            category: ThingUpdateCategory.General,
            // @@NOTE: Using DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() could sometimes result in
            // equal values for 'lastSeenUpdateTimestamp' and 'updateTimestamp'.
            updateTimestamp: now + 10,
            title: "Draft created",
            details: "Click to open the newly created draft"
        ));

        await _thingRepository.SaveChanges();
        await _watchedItemRepository.SaveChanges();
        await _thingUpdateRepository.SaveChanges();

        _logger.LogInformation($"Thing (Id: {ThingId}, Title: {ThingTitle}) created", thing.Id, thing.Title);
    }
}

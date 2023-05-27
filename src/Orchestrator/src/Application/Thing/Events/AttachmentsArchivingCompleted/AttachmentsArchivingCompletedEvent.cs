using MediatR;

using Domain.Aggregates;
using ThingDm = Domain.Aggregates.Thing;

using Application.Thing.Common.Models.IM;
using Application.Common.Interfaces;
using Application.Common.Attributes;

namespace Application.Thing.Events.AttachmentsArchivingCompleted;

[ExecuteInTxn]
public class AttachmentsArchivingCompletedEvent : INotification
{
    public required string SubmitterId { get; init; }
    public required Guid ThingId { get; init; }
    public required NewThingIm Input { get; init; }
}

internal class AttachmentsArchivingCompletedEventHandler : INotificationHandler<AttachmentsArchivingCompletedEvent>
{
    private readonly IClientNotifier _clientNotifier;
    private readonly IThingRepository _thingRepository;
    private readonly IThingUpdateRepository _thingUpdateRepository;
    private readonly IWatchedItemRepository _watchedItemRepository;

    public AttachmentsArchivingCompletedEventHandler(
        IClientNotifier clientNotifier,
        IThingRepository thingRepository,
        IThingUpdateRepository thingUpdateRepository,
        IWatchedItemRepository watchedItemRepository
    )
    {
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
            return new Evidence(
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
    }
}
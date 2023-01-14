using MediatR;

using Domain.Aggregates;
using ThingDm = Domain.Aggregates.Thing;

using Application.Thing.Common.Models.IM;
using Application.Common.Interfaces;

namespace Application.Thing.Events.AttachmentsArchivingCompleted;

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

    public AttachmentsArchivingCompletedEventHandler(
        IClientNotifier clientNotifier,
        IThingRepository thingRepository
    )
    {
        _clientNotifier = clientNotifier;
        _thingRepository = thingRepository;
    }

    public async Task Handle(AttachmentsArchivingCompletedEvent @event, CancellationToken ct)
    {
        await _clientNotifier.TellAboutNewThingDraftCreationProgress(@event.SubmitterId, @event.ThingId, percent: 100);

        var thing = new ThingDm(
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

        await _thingRepository.SaveChanges();
    }
}
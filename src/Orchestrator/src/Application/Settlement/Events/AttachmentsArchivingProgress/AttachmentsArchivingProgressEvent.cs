using MediatR;

using Application.Common.Interfaces;

namespace Application.Settlement.Events.AttachmentsArchivingProgress;

public class AttachmentsArchivingProgressEvent : INotification
{
    public required string SubmitterId { get; init; }
    public required Guid ProposalId { get; init; }
    public required int Percent { get; init; }
}

internal class AttachmentsArchivingProgressEventHandler : INotificationHandler<AttachmentsArchivingProgressEvent>
{
    private readonly IClientNotifier _clientNotifier;

    public AttachmentsArchivingProgressEventHandler(IClientNotifier clientNotifier)
    {
        _clientNotifier = clientNotifier;
    }

    public Task Handle(AttachmentsArchivingProgressEvent @event, CancellationToken ct) =>
        _clientNotifier.TellAboutNewSettlementProposalDraftCreationProgress(
            @event.SubmitterId, @event.ProposalId, percent: @event.Percent
        );
}
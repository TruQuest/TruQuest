using GoThataway;

using Application.Common.Interfaces;
using Application.Common.Monitoring;

namespace Application.Settlement.Events.AttachmentsArchivingProgress;

public class AttachmentsArchivingProgressEvent : IEvent
{
    public required string SubmitterId { get; init; }
    public required Guid ProposalId { get; init; }
    public required int Percent { get; init; }

    public IEnumerable<(string Name, object? Value)> GetActivityTags()
    {
        return new (string Name, object? Value)[]
        {
            (ActivityTags.UserId, SubmitterId),
            (ActivityTags.SettlementProposalId, ProposalId)
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
        _clientNotifier.TellAboutNewSettlementProposalDraftCreationProgress(
            @event.SubmitterId, @event.ProposalId, percent: @event.Percent
        );
}

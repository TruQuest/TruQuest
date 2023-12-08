using Microsoft.Extensions.Logging;

using GoThataway;

using Application.Common.Interfaces;

namespace Application.Settlement.Events.AttachmentsArchivingFailed;

public class AttachmentsArchivingFailedEvent : IEvent
{
    public required string SubmitterId { get; init; }
    public required Guid ProposalId { get; init; }
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
            "Error trying to archive attachments for settlement proposal {SettlementProposalId}: " + @event.ErrorMessage,
            @event.ProposalId
        );

        await _clientNotifier.TellAboutNewSettlementProposalDraftCreationProgress(@event.SubmitterId, @event.ProposalId, -100);
    }
}

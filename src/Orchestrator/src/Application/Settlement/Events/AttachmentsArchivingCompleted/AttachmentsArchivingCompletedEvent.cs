using MediatR;

using Domain.Aggregates;

using Application.Common.Interfaces;
using Application.Settlement.Common.Models.IM;
using Application.Common.Attributes;

namespace Application.Settlement.Events.AttachmentsArchivingCompleted;

[ExecuteInTxn]
public class AttachmentsArchivingCompletedEvent : INotification
{
    public required string SubmitterId { get; init; }
    public required Guid ProposalId { get; init; }
    public required NewSettlementProposalIm Input { get; init; }
}

internal class AttachmentsArchivingCompletedEventHandler : INotificationHandler<AttachmentsArchivingCompletedEvent>
{
    private readonly IClientNotifier _clientNotifier;
    private readonly ISettlementProposalRepository _settlementProposalRepository;
    private readonly ISettlementProposalUpdateRepository _settlementProposalUpdateRepository;
    private readonly IWatchedItemRepository _watchedItemRepository;

    public AttachmentsArchivingCompletedEventHandler(
        IClientNotifier clientNotifier,
        ISettlementProposalRepository settlementProposalRepository,
        ISettlementProposalUpdateRepository settlementProposalUpdateRepository,
        IWatchedItemRepository watchedItemRepository
    )
    {
        _clientNotifier = clientNotifier;
        _settlementProposalRepository = settlementProposalRepository;
        _settlementProposalUpdateRepository = settlementProposalUpdateRepository;
        _watchedItemRepository = watchedItemRepository;
    }

    public async Task Handle(AttachmentsArchivingCompletedEvent @event, CancellationToken ct)
    {
        await _clientNotifier.TellAboutNewSettlementProposalDraftCreationProgress(
            @event.SubmitterId, @event.ProposalId, percent: 100
        );

        var proposal = new SettlementProposal(
            id: @event.ProposalId,
            thingId: @event.Input.ThingId,
            title: @event.Input.Title,
            verdict: (Verdict)@event.Input.Verdict,
            details: @event.Input.Details,
            imageIpfsCid: @event.Input.ImageIpfsCid,
            croppedImageIpfsCid: @event.Input.CroppedImageIpfsCid,
            submitterId: @event.SubmitterId
        );
        proposal.AddEvidence(@event.Input.Evidence.Select(e =>
        {
            return new SettlementProposalEvidence(
                originUrl: e.Url,
                ipfsCid: e.IpfsCid!,
                previewImageIpfsCid: e.PreviewImageIpfsCid!
            );
        }));

        _settlementProposalRepository.Create(proposal);

        var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        _watchedItemRepository.Add(new WatchedItem(
            userId: @event.SubmitterId,
            itemType: WatchedItemType.SettlementProposal,
            itemId: proposal.Id,
            itemUpdateCategory: (int)SettlementProposalUpdateCategory.General,
            lastSeenUpdateTimestamp: now
        ));

        await _settlementProposalUpdateRepository.AddOrUpdate(new SettlementProposalUpdate(
            settlementProposalId: proposal.Id,
            category: SettlementProposalUpdateCategory.General,
            updateTimestamp: now + 10,
            title: "Draft created",
            details: "Click to open the newly created draft"
        ));

        await _settlementProposalRepository.SaveChanges();
        await _watchedItemRepository.SaveChanges();
        await _settlementProposalUpdateRepository.SaveChanges();
    }
}

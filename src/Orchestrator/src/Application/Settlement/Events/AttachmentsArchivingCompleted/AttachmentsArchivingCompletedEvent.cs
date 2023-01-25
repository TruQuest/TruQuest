using MediatR;

using Domain.Aggregates;

using Application.Common.Interfaces;
using Application.Settlement.Common.Models.IM;

namespace Application.Settlement.Events.AttachmentsArchivingCompleted;

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

    public AttachmentsArchivingCompletedEventHandler(
        IClientNotifier clientNotifier,
        ISettlementProposalRepository settlementProposalRepository
    )
    {
        _clientNotifier = clientNotifier;
        _settlementProposalRepository = settlementProposalRepository;
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
            return new SupportingEvidence(
                originUrl: e.Url,
                ipfsCid: e.IpfsCid!,
                previewImageIpfsCid: e.PreviewImageIpfsCid!
            );
        }));

        _settlementProposalRepository.Create(proposal);

        await _settlementProposalRepository.SaveChanges();
    }
}
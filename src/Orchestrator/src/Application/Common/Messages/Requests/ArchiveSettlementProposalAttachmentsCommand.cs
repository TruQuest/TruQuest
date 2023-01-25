using Application.Settlement.Common.Models.IM;

namespace Application.Common.Messages.Requests;

public class ArchiveSettlementProposalAttachmentsCommand
{
    public required string SubmitterId { get; init; }
    public required Guid ProposalId { get; init; }
    public required NewSettlementProposalIm Input { get; init; }
}
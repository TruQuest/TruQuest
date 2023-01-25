using Application.Settlement.Common.Models.IM;

namespace Application.Common.Messages.Responses;

public class ArchiveSettlementProposalAttachmentsSuccessResult
{
    public required string SubmitterId { get; init; }
    public required Guid ProposalId { get; init; }
    public required NewSettlementProposalIm Input { get; init; }
}
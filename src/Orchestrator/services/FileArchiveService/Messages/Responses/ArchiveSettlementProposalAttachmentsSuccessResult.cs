using Messages.Requests;

namespace Messages.Responses;

internal class ArchiveSettlementProposalAttachmentsSuccessResult
{
    public required string SubmitterId { get; init; }
    public required Guid ProposalId { get; init; }
    public required NewSettlementProposalIm Input { get; init; }
}
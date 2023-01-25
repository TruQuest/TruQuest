namespace Application.Common.Messages.Responses;

public class ArchiveSettlementProposalAttachmentsProgress
{
    public required string SubmitterId { get; init; }
    public required Guid ProposalId { get; init; }
    public required int Percent { get; init; }
}
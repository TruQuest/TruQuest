using Common.Monitoring;

namespace Messages.Responses;

internal class ArchiveSettlementProposalAttachmentsFailureResult : BaseResponse
{
    public required string SubmitterId { get; init; }
    public required Guid ProposalId { get; init; }
    public required string ErrorMessage { get; init; }

    public override IEnumerable<(string Name, object? Value)> GetActivityTags()
    {
        return new (string Name, object? Value)[]
        {
            (ActivityTags.UserId, SubmitterId),
            (ActivityTags.SettlementProposalId, ProposalId)
        };
    }
}

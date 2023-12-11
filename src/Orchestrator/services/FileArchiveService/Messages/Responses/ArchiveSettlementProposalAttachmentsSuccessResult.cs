using Common.Monitoring;
using Messages.Requests;

namespace Messages.Responses;

internal class ArchiveSettlementProposalAttachmentsSuccessResult : BaseResponse
{
    public required string SubmitterId { get; init; }
    public required Guid ProposalId { get; init; }
    public required NewSettlementProposalIm Input { get; init; }

    public override IEnumerable<(string Name, object? Value)> GetActivityTags()
    {
        return new (string Name, object? Value)[]
        {
            (ActivityTags.UserId, SubmitterId),
            (ActivityTags.ThingId, Input.ThingId),
            (ActivityTags.SettlementProposalId, ProposalId)
        };
    }
}

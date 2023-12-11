using Application.Common.Monitoring;
using Application.Settlement.Common.Models.IM;

namespace Application.Common.Messages.Requests;

public class ArchiveSettlementProposalAttachmentsCommand : BaseRequest
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

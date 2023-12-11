using Common.Monitoring;

namespace Messages.Responses;

internal class ArchiveSettlementProposalAttachmentsProgress : BaseResponse
{
    public required string SubmitterId { get; init; }
    public required Guid ProposalId { get; init; }
    public required int Percent { get; init; }

    public override IEnumerable<(string Name, object? Value)> GetActivityTags()
    {
        return new (string Name, object? Value)[]
        {
            (ActivityTags.UserId, SubmitterId),
            (ActivityTags.SettlementProposalId, ProposalId)
        };
    }
}

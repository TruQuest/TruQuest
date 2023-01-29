namespace Application.Settlement.Commands.CastAssessmentPollVote;

public class NewAssessmentPollVoteIm
{
    public required Guid ThingId { get; init; }
    public Guid SettlementProposalId { get; set; }
    public required string CastedAt { get; init; }
    public required DecisionIm Decision { get; init; }
    public required string Reason { get; init; }
}
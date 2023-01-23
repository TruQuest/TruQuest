namespace Application.Thing.Commands.CastAcceptancePollVote;

public class NewAcceptancePollVoteIm
{
    public Guid ThingId { get; set; }
    public required string CastedAt { get; init; }
    public required DecisionIm Decision { get; init; }
    public required string Reason { get; init; }
}
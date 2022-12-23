namespace Application.Vote.Commands.CastAcceptancePollVote;

public class NewAcceptancePollVoteIm
{
    public Guid ThingId { get; set; }
    public string CastedAt { get; set; }
    public DecisionIm Decision { get; set; }
    public string Reason { get; set; } = string.Empty;
}
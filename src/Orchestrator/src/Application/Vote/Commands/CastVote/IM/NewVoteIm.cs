namespace Application.Vote.Commands.CastVote;

public class NewVoteIm
{
    public Guid ThingId { get; set; }
    public PollTypeIm PollType { get; set; }
    public string CastedAt { get; set; }
    public DecisionIm Decision { get; set; }
    public string Reason { get; set; } = string.Empty;
}
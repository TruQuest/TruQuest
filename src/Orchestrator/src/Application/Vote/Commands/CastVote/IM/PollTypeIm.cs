namespace Application.Vote.Commands.CastVote;

public enum PollTypeIm
{
    Acceptance,
}

public static class PollTypeImExtension
{
    public static string GetString(this PollTypeIm pollType) => pollType.ToString();
}
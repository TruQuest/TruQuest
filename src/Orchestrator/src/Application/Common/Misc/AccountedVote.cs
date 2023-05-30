namespace Application.Common.Misc;

internal class AccountedVote
{
    internal enum Decision
    {
        SoftDecline,
        HardDecline,
        Accept
    }

    public required string VoterId { get; init; }
    public required Decision VoteDecision { get; init; }

    public override bool Equals(object? obj)
    {
        var other = obj as AccountedVote;
        if (other == null) return false;
        return VoterId == other.VoterId;
    }

    public override int GetHashCode() => VoterId.GetHashCode();
}

internal static class DecisionExtension
{
    public static int GetScore(this AccountedVote.Decision decision) =>
        decision == AccountedVote.Decision.Accept ? 1 : 0;
}
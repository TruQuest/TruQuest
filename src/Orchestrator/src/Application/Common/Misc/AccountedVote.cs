namespace Application.Common.Misc;

internal class AccountedVote
{
    public required string VoterId { get; init; }
    public required int Decision { get; init; }

    public override bool Equals(object? obj)
    {
        var other = obj as AccountedVote;
        if (other == null) return false;
        return VoterId == other.VoterId;
    }

    public override int GetHashCode() => VoterId.GetHashCode();
}
using System.Text.Json.Serialization;

namespace Application.Common.Models.QM;

public class VerifierLotteryParticipantEntryQm
{
    public required long L1BlockNumber { get; init; }
    public required string TxnHash { get; init; }
    public required string UserId { get; init; }
    public required string UserData { get; init; }
    public required long? Nonce { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? IsWinner { get; private set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
    public int SortKey => IsWinner != null ? 0 : 1;

    public override bool Equals(object? obj)
    {
        var other = obj as VerifierLotteryParticipantEntryQm;
        if (other == null) return false;
        return UserId == other.UserId;
    }

    public override int GetHashCode() => UserId.GetHashCode();

    public void MarkAsWinner() => IsWinner = true;
}

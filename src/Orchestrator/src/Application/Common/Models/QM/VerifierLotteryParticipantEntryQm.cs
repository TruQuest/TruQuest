using System.Text.Json.Serialization;

namespace Application.Common.Models.QM;

public class VerifierLotteryParticipantEntryQm
{
    public long L1BlockNumber { get; }
    public long BlockNumber { get; }
    public required string TxnHash { get; init; }
    public required string UserId { get; init; }
    public string? UserData { get; }
    public long? Nonce { get; private set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? IsOrchestrator { get; private set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? IsWinner { get; private set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
    public int SortKey => IsOrchestrator != null ? 0 : IsWinner != null ? 1 : 2;

    public override bool Equals(object? obj)
    {
        var other = obj as VerifierLotteryParticipantEntryQm;
        if (other == null) return false;
        return UserId == other.UserId;
    }

    public override int GetHashCode() => UserId.GetHashCode();

    public void MarkAsOrchestrator() => IsOrchestrator = true;

    public void MarkAsWinner() => IsWinner = true;

    public void ClearSensitiveData()
    {
        Nonce = null;
    }
}

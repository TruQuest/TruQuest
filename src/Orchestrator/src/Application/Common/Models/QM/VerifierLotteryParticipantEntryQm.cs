using System.Text.Json.Serialization;

namespace Application.Common.Models.QM;

public class VerifierLotteryParticipantEntryQm
{
    public long? JoinedBlockNumber { get; }
    public string UserId { get; }
    public string DataHash { get; }
    public decimal? Nonce { get; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? IsOrchestrator { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? IsWinner { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
    public int SortKey => IsOrchestrator != null ? 0 : IsWinner != null ? 1 : 2;
}
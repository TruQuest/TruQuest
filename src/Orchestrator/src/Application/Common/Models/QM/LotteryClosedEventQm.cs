using System.Text.Json.Serialization;

using Domain.Aggregates.Events;

namespace Application.Common.Models.QM;

public class LotteryClosedEventQm
{
    public required string TxnHash { get; init; }
    // @@NOTE: For some reason specifying both Ignore.Always and 'required'
    // doesn't work (throws "Marked as required but has no setter" when serializing).
    // Without 'required' works fine.
    [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
    public ThingEventType Type { get; init; }
    [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
    public Dictionary<string, object> Payload { get; init; }
    public required long? Nonce { get; init; }
}

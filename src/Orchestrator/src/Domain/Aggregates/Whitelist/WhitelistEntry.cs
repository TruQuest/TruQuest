using Domain.Base;

namespace Domain.Aggregates;

public class WhitelistEntry : Entity, IAggregateRoot
{
    public WhitelistEntryType Type { get; }
    public string Value { get; }

    public WhitelistEntry(WhitelistEntryType type, string value)
    {
        Type = type;
        Value = value;
    }
}

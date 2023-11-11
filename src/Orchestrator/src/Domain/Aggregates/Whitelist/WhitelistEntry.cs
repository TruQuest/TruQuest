using Domain.Base;

namespace Domain.Aggregates;

public class WhitelistEntry : Entity, IAggregateRoot
{
    public int? Id { get; private set; }
    public WhitelistEntryType Type { get; }
    public string Value { get; }

    public WhitelistEntry(WhitelistEntryType type, string value)
    {
        Type = type;
        Value = value;
    }
}

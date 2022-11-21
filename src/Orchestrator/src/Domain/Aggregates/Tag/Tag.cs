using Domain.Base;

namespace Domain.Aggregates;

public class Tag : Entity, IAggregateRoot
{
    public int? Id { get; private set; }
    public string Name { get; }

    public Tag(string name)
    {
        Name = name;
    }
}
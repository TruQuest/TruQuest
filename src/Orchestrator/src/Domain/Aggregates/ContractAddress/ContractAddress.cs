using Domain.Base;

namespace Domain.Aggregates;

public class ContractAddress : Entity, IAggregateRoot
{
    public string Name { get; }
    public string Version { get; }
    public string Address { get; }

    public ContractAddress(string name, string version, string address)
    {
        Name = name;
        Version = version;
        Address = address;
    }
}

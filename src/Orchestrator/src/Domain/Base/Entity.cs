using GoThataway;

namespace Domain.Base;

public abstract class Entity
{
    private List<IEvent>? _domainEvents;
    public IReadOnlyList<IEvent>? DomainEvents => _domainEvents;

    public void AddDomainEvent(IEvent @event)
    {
        _domainEvents ??= new List<IEvent>();
        _domainEvents.Add(@event);
    }

    public void RemoveDomainEvent(IEvent @event)
    {
        _domainEvents?.Remove(@event);
    }

    public void ClearDomainEvents()
    {
        _domainEvents?.Clear();
    }
}

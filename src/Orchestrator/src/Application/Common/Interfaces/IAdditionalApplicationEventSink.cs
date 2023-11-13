using GoThataway;

namespace Application.Common.Interfaces;

public interface IAdditionalApplicationEventSink
{
    void Add(IEvent @event);
}

using MediatR;

namespace Application.Common.Interfaces;

public interface IAdditionalApplicationEventSink
{
    void Add(INotification @event);
}

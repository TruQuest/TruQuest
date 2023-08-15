using MediatR;

namespace Application.Common.Interfaces;

public interface IAdditionalApplicationEventSink
{
    ValueTask Add(INotification @event);
}

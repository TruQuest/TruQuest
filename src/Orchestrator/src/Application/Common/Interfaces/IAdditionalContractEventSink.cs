using MediatR;

namespace Application.Common.Interfaces;

public interface IAdditionalContractEventSink
{
    ValueTask Add(INotification @event);
}
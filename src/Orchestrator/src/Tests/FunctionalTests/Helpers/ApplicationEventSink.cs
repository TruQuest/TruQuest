using System.Threading.Channels;

using MediatR;

using Application.Common.Interfaces;

namespace Tests.FunctionalTests.Helpers;

public class ApplicationEventSink : IAdditionalApplicationEventSink
{
    private Channel<INotification> _eventChannel;
    public ChannelReader<INotification> Stream => _eventChannel.Reader;

    public ValueTask Add(INotification @event) => _eventChannel.Writer.WriteAsync(@event);

    public void Reset()
    {
        _eventChannel = Channel.CreateUnbounded<INotification>();
    }
}

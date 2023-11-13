using System.Threading.Channels;

using GoThataway;

using Application.Common.Interfaces;

namespace Tests.FunctionalTests.Helpers;

public class ApplicationEventChannel : IAdditionalApplicationEventSink
{
    private readonly List<ChannelWriter<IEvent>> _consumerSinks = new();

    public void Add(IEvent @event)
    {
        lock (_consumerSinks) // @@NOTE: This is only used by tests so locking everything is fine.
        {
            foreach (var sink in _consumerSinks)
            {
                sink.TryWrite(@event);
            }
        }
    }

    public void RegisterConsumer(ChannelWriter<IEvent> consumerSink)
    {
        lock (_consumerSinks)
        {
            _consumerSinks.Add(consumerSink);
        }
    }

    public void UnregisterConsumer(ChannelWriter<IEvent> consumerSink)
    {
        lock (_consumerSinks)
        {
            _consumerSinks.Remove(consumerSink);
        }
    }
}

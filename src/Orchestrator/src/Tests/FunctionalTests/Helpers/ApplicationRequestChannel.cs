using System.Threading.Channels;

using MediatR;

using Application.Common.Interfaces;

namespace Tests.FunctionalTests.Helpers;

public class ApplicationRequestChannel : IAdditionalApplicationRequestSink
{
    private readonly List<ChannelWriter<IBaseRequest>> _consumerSinks = new();

    public void Add(IBaseRequest request)
    {
        lock (_consumerSinks) // @@NOTE: This is only used by tests so locking everything is fine.
        {
            foreach (var sink in _consumerSinks)
            {
                sink.TryWrite(request);
            }
        }
    }

    public void RegisterConsumer(ChannelWriter<IBaseRequest> consumerSink)
    {
        lock (_consumerSinks)
        {
            _consumerSinks.Add(consumerSink);
        }
    }

    public void UnregisterConsumer(ChannelWriter<IBaseRequest> consumerSink)
    {
        lock (_consumerSinks)
        {
            _consumerSinks.Remove(consumerSink);
        }
    }
}

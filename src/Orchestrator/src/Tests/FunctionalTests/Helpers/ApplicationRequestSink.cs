using System.Threading.Channels;

using MediatR;

using Application.Common.Interfaces;

namespace Tests.FunctionalTests.Helpers;

public class ApplicationRequestSink : IAdditionalApplicationRequestSink
{
    private Channel<IBaseRequest> _requestChannel;
    public ChannelReader<IBaseRequest> Stream => _requestChannel.Reader;

    public ValueTask Add(IBaseRequest request) => _requestChannel.Writer.WriteAsync(request);

    public void Reset()
    {
        _requestChannel = Channel.CreateUnbounded<IBaseRequest>();
    }
}

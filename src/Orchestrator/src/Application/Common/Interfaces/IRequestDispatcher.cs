using System.Threading.Channels;

namespace Application.Common.Interfaces;

public interface IRequestDispatcher
{
    ChannelWriter<(string RequestId, object Message)> ResponseSink { get; }
    Task<object> GetResult(object request);
    Task Send(object request);
}

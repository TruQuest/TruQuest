using System.Threading.Channels;

namespace Application.Common.Interfaces;

public interface IRequestDispatcher
{
    ChannelWriter<(string RequestId, object Message)> ResponseSink { get; }
    Task<object> GetResult(object request, string? requestId = null);
    Task Send(object request, string? requestId = null);
}

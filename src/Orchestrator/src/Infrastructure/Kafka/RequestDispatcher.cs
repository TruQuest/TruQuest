using System.Collections.Concurrent;
using System.Text;

using KafkaFlow;

using Application.Common.Interfaces;

namespace Infrastructure.Kafka;

internal class RequestDispatcher : IRequestDispatcher
{
    private readonly IMessageProducer<RequestDispatcher> _producer;

    private readonly ConcurrentDictionary<string, TaskCompletionSource<object>> _requestIdToResponseReceivedTcs = new();

    public RequestDispatcher(IMessageProducer<RequestDispatcher> producer)
    {
        _producer = producer;
    }

    public void SetResponseFor(string requestId, object response)
    {
        if (_requestIdToResponseReceivedTcs.TryRemove(requestId, out TaskCompletionSource<object>? tcs))
        {
            tcs.SetResult(response);
        }
    }

    public async Task<object> GetResult(object request)
    {
        var requestId = Guid.NewGuid().ToString();
        var tcs = _requestIdToResponseReceivedTcs[requestId] = new();

        await _producer.ProduceAsync(
            messageKey: Guid.NewGuid().ToString(),
            messageValue: request,
            headers: new MessageHeaders
            {
                ["requestId"] = Encoding.UTF8.GetBytes(requestId)
            }
        );

        var result = await tcs.Task;

        return result;
    }

    public async Task Send(object request)
    {
        await _producer.ProduceAsync(
            messageKey: Guid.NewGuid().ToString(),
            messageValue: request
        );
    }
}
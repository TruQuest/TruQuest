using System.Diagnostics;

using Microsoft.Extensions.Caching.Memory;

using GoThataway;

using Application.Ethereum.Common.Models.IM;
using Application.Ethereum.Events.BlockMined;

namespace Application.Common.Middlewares.Event;

public class TracingMiddleware<TEvent> : IEventMiddleware<TEvent> where TEvent : IEvent
{
    private readonly IMemoryCache _memoryCache;

    public TracingMiddleware(IMemoryCache memoryCache)
    {
        _memoryCache = memoryCache;
    }

    public async Task Handle(TEvent @event, Func<Task> next, CancellationToken ct)
    {
        Activity? span = null;
        try
        {
            if (@event is not BlockMinedEvent)
            {
                string? traceparent = null;
                if (@event is BaseContractEvent contractEvent)
                {
                    // @@!!: Cache entry is added once txn receipt is received, therefore, when listening
                    // for events we must wait for more confirmations than when we send the txn.
                    traceparent = _memoryCache.Get<string>(contractEvent.TxnHash);
                }
                span = Telemetry.StartActivity(@event.GetType().FullName!, traceparent: traceparent)!;
            }

            await next();
        }
        finally
        {
            span?.Dispose();
        }
    }
}

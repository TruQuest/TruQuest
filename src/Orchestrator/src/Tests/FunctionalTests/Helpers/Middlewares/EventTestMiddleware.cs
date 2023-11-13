using GoThataway;

using Application.Common.Interfaces;
using Application.Ethereum.Events.BlockMined;

namespace Tests.FunctionalTests.Helpers.Middlewares;

public class EventTestMiddleware<TEvent> : IEventMiddleware<TEvent> where TEvent : IEvent
{
    private readonly IAdditionalApplicationEventSink _sink;

    public EventTestMiddleware(IAdditionalApplicationEventSink sink)
    {
        _sink = sink;
    }

    public async Task Handle(TEvent @event, Func<Task> next, CancellationToken ct)
    {
        await next();
        if (@event is not BlockMinedEvent) _sink.Add(@event);
    }
}

using Microsoft.Extensions.Logging;

using GoThataway;

namespace Application.Common.Middlewares.Event;

public class ExceptionLoggingMiddleware<TEvent> : IEventMiddleware<TEvent> where TEvent : IEvent
{
    private readonly ILogger<ExceptionLoggingMiddleware<TEvent>> _logger;

    public ExceptionLoggingMiddleware(ILogger<ExceptionLoggingMiddleware<TEvent>> logger)
    {
        _logger = logger;
    }

    public async Task Handle(TEvent @event, Func<Task> next, CancellationToken ct)
    {
        // @@??: If we just rethrow, is there a need for this middleware in the first place?
        try
        {
            await next();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handing event: {Event}", @event.GetType().FullName);
            throw;
            // @@TODO!!: What to do if an exception gets thrown during event processing? It could mean,
            // for example, an ethereum event not being stored in the db (and, consequently, it not
            // triggering a command, if the event in question is actionable). We should really have
            // a retry-or-archive mechanism for events as well.
        }
    }
}

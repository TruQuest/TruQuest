using System.Reflection;
using System.Transactions;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Npgsql;
using GoThataway;

using Application.Common.Attributes;

namespace Infrastructure.Persistence;

public class EventTransactionMiddleware<TEvent> : IEventMiddleware<TEvent> where TEvent : IEvent
{
    private readonly ILogger<EventTransactionMiddleware<TEvent>> _logger;

    public EventTransactionMiddleware(ILogger<EventTransactionMiddleware<TEvent>> logger)
    {
        _logger = logger;
    }

    public async Task Handle(TEvent @event, Func<Task> next, CancellationToken ct)
    {
        // @@NOTE: The only events that have ExecuteInTxnAttribute are two AttachmentsArchivingCompletedEvents,
        // both of which are just a bunch of INSERTs, meaning there couldn't (@@??) be a serialization failure.

        try
        {
            var attr = @event.GetType().GetCustomAttribute<ExecuteInTxnAttribute>();
            if (attr == null) await next();
            else
            {
                using var dbConn = DbConnectionProvider.Init();

                using var txnScope = new TransactionScope(
                    TransactionScopeOption.Required,
                    new TransactionOptions { IsolationLevel = attr.IsolationLevel },
                    TransactionScopeAsyncFlowOption.Enabled
                );

                await next();
                txnScope.Complete();
            }
        }
        catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            _logger.LogWarning(
                ex,
                "Unique constraint violation while processing {Event}. Skipping...",
                @event.GetType().Name
            );
        }
        catch (DbUpdateException ex) when (
            ex.InnerException is PostgresException pgEx &&
            pgEx.SqlState == PostgresErrorCodes.UniqueViolation
        )
        {
            _logger.LogWarning(
                pgEx,
                "Unique constraint violation while processing {Event}. Skipping...",
                @event.GetType().Name
            );
        }
    }
}

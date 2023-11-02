using System.Diagnostics;
using System.Reflection;
using System.Transactions;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;

using Npgsql;
using MediatR;

using Application;
using Application.Common.Attributes;
using Application.Common.Interfaces;
using Application.Ethereum.Events.BlockMined;
using Application.Ethereum.Common.Models.IM;

using Infrastructure.Persistence;

namespace Infrastructure;

public class PublisherWrapper
{
    private readonly ILogger<PublisherWrapper> _logger;
    private readonly IPublisher _publisher;
    private readonly IMemoryCache _memoryCache;
    private readonly IEnumerable<IAdditionalApplicationEventSink> _additionalSinks;

    public PublisherWrapper(
        ILogger<PublisherWrapper> logger,
        IPublisher publisher,
        IMemoryCache memoryCache,
        IEnumerable<IAdditionalApplicationEventSink> additionalSinks
    )
    {
        _logger = logger;
        _publisher = publisher;
        _memoryCache = memoryCache;
        _additionalSinks = additionalSinks;
    }

    public async Task Publish(INotification @event, CancellationToken ct = default, bool addToAdditionalSinks = false)
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
                    // for events we must wait for more confirmations than when we send a txn.
                    traceparent = _memoryCache.Get<string>(contractEvent.TxnHash);
                }
                span = Telemetry.StartActivity(@event.GetType().FullName!, traceparent: traceparent)!;
            }

            var attr = @event.GetType().GetCustomAttribute<ExecuteInTxnAttribute>();
            if (attr != null)
            {
                // @@NOTE: The only events that have ExecuteInTxnAttribute are two AttachmentsArchivingCompletedEvents,
                // both of which are just a bunch of INSERTs, meaning there couldn't (@@??) be a serialization failure.

                using var dbConn = DbConnectionProvider.Init();

                using var txnScope = new TransactionScope(
                    TransactionScopeOption.Required,
                    new TransactionOptions { IsolationLevel = attr.IsolationLevel },
                    TransactionScopeAsyncFlowOption.Enabled
                );

                try
                {
                    await _publisher.Publish(@event);
                    txnScope.Complete();
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
            else
            {
                try
                {
                    await _publisher.Publish(@event);
                }
                catch (PostgresException ex) when (
                    ex.SqlState == PostgresErrorCodes.UniqueViolation
                )
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

            if (addToAdditionalSinks)
            {
                foreach (var sink in _additionalSinks)
                {
                    await sink.Add(@event);
                }
            }
        }
        finally
        {
            span?.Dispose();
        }
    }
}

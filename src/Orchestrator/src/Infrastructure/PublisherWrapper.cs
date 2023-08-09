using System.Reflection;
using System.Transactions;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Npgsql;
using MediatR;

using Application.Common.Attributes;
using Application.Common.Interfaces;

namespace Infrastructure;

public class PublisherWrapper
{
    private readonly ILogger<PublisherWrapper> _logger;
    private readonly ISharedTxnScope _sharedTxnScope;
    private readonly IPublisher _publisher;
    private readonly IEnumerable<IAdditionalContractEventSink> _additionalSinks;

    public PublisherWrapper(
        ILogger<PublisherWrapper> logger,
        ISharedTxnScope sharedTxnScope,
        IPublisher publisher,
        IEnumerable<IAdditionalContractEventSink> additionalSinks
    )
    {
        _logger = logger;
        _sharedTxnScope = sharedTxnScope;
        _publisher = publisher;
        _additionalSinks = additionalSinks;
    }

    public async Task Publish(
        INotification @event, CancellationToken ct = default, bool addToAdditionalSinks = true
    )
    {
        var attr = @event.GetType().GetCustomAttribute<ExecuteInTxnAttribute>();
        if (attr != null)
        {
            // @@NOTE: The only events that have ExecuteInTxnAttribute are two AttachmentsArchivingCompletedEvents,
            // both of which are just a bunch of INSERTs, meaning there couldn't (@@??) be a serialization failure.

            _sharedTxnScope.Init();

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
}

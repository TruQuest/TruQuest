using System.Reflection;
using System.Transactions;

using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

using MediatR;
using Npgsql;
using OpenTelemetry.Trace;

using Domain.Results;
using Application;
using Application.Common.Attributes;
using Application.Common.Errors;

namespace Infrastructure.Persistence;

public class TransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : HandleResult, new()
{
    private readonly ILogger<TransactionBehavior<TRequest, TResponse>> _logger;

    public TransactionBehavior(ILogger<TransactionBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        var attr = request.GetType().GetCustomAttribute<ExecuteInTxnAttribute>();
        if (attr == null) return await next();

        // @@NOTE: We do not (cannot) retry the next() call in here, since a new DbConnection
        // added to AsyncLocal won't propagate to scoped repositories (since they won't be
        // created anew, the same instances will be reused). And even if we make them transient, the
        // underlying DbContext will still be scoped with an already open connection, so we won't
        // be allowed to set another connection on it. And making DbContext transient is a no-go
        // since we want it to be the same instance between multiple repos and queryables.

        // So the burden of retrying is on the caller, i.e. in case of a DeferredTaskCommand the task
        // system will repeat the task execution since it won't be marked as completed, and in case of
        // a command originating from kafka event the RetryOrArchiveMiddleware will take care of it.

        try
        {
            using var dbConn = DbConnectionProvider.Init();

            using var txnScope = new TransactionScope(
                TransactionScopeOption.Required,
                new TransactionOptions { IsolationLevel = attr.IsolationLevel },
                TransactionScopeAsyncFlowOption.Enabled
            );

            var response = await next();
            if (response.Error == null) txnScope.Complete();

            return response;
        }
        catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.SerializationFailure)
        {
            _logger.LogWarning(ex, ex.Message);
            Telemetry.CurrentActivity!.RecordException(ex);
        }
        catch (DbUpdateException ex) when
        (
            ex.InnerException is PostgresException pgEx &&
            pgEx.SqlState == PostgresErrorCodes.SerializationFailure
        )
        {
            _logger.LogWarning(pgEx, pgEx.Message);
            Telemetry.CurrentActivity!.RecordException(pgEx);
        }
        // @@??: Why DbUpdateException gets wrapped into InvalidOperationException ?
        catch (InvalidOperationException ex) when
        (
            ex.InnerException is DbUpdateException dbEx &&
            dbEx.InnerException is PostgresException pgEx &&
            pgEx.SqlState == PostgresErrorCodes.SerializationFailure
        )
        {
            _logger.LogWarning(pgEx, pgEx.Message);
            Telemetry.CurrentActivity!.RecordException(pgEx);
        }

        return new TResponse
        {
            Error = new ServerError("Transaction serialization failure", isRetryable: true)
        };
    }
}

using System.Reflection;
using System.Transactions;

using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

using GoThataway;
using Npgsql;

using Domain.Results;
using Domain.Errors;
using Application;
using Application.Common.Attributes;

namespace Infrastructure.Persistence;

public class RequestTransactionMiddleware<TRequest, TResponse> : IRequestMiddleware<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : HandleResult, new()
{
    private readonly ILogger<RequestTransactionMiddleware<TRequest, TResponse>> _logger;

    public RequestTransactionMiddleware(ILogger<RequestTransactionMiddleware<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, Func<Task<TResponse>> next, CancellationToken ct)
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
        }
        catch (DbUpdateException ex) when
        (
            ex.InnerException is PostgresException pgEx &&
            pgEx.SqlState == PostgresErrorCodes.SerializationFailure
        )
        {
            _logger.LogWarning(pgEx, pgEx.Message);
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
        }

        return new TResponse
        {
            Error = new UnhandledError(
                "Transaction serialization failure",
                Telemetry.CurrentActivity!.TraceId.ToHexString(),
                isRetryable: true
            )
        };
    }
}
